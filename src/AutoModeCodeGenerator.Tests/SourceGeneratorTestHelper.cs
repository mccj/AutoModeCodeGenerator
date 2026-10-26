using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using AutoModeCodeGenerator.Internal;
using AutoModeCodeGenerator.Internal.EntityFiles;

namespace AutoModeCodeGenerator.Tests;

internal sealed class InMemoryAdditionalText(string path, string content) : AdditionalText
{
    private readonly SourceText _sourceText = SourceText.From(content, Encoding.UTF8);

    public override string Path { get; } = path;

    public override SourceText GetText(CancellationToken cancellationToken = default) => _sourceText;
}

internal sealed class TestAnalyzerConfigOptions(Dictionary<string, string> values) : AnalyzerConfigOptions
{
    public override bool TryGetValue(string key, out string value)
        => values.TryGetValue(key, out value!);
}

internal sealed class TestAnalyzerConfigOptionsProvider(Dictionary<string, string> globalValues) : AnalyzerConfigOptionsProvider
{
    private readonly TestAnalyzerConfigOptions _global = new(globalValues);

    public override AnalyzerConfigOptions GlobalOptions => _global;

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => _global;

    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => _global;
}

internal static class SourceGeneratorTestHelper
{
    private static readonly MetadataReference[] References = CreateReferences();

    private static MetadataReference[] CreateReferences()
    {
        var refs = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.DisplayNameAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.INotifyPropertyChanged).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).Assembly.Location),
        };

        // 内存编译需要 System.Runtime 外观程序集，才能解析 MulticastDelegate / 事件等。
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        foreach (var name in new[] { "System.Runtime.dll", "netstandard.dll", "System.Collections.dll" })
        {
            var path = Path.Combine(runtimeDir, name);
            if (File.Exists(path))
                refs.Add(MetadataReference.CreateFromFile(path));
        }

        return refs.ToArray();
    }

    public static MetadataReference[] GetMetadataReferences() => References;

    public static GeneratorRunResult RunGenerator(
        string source,
        string fileName = "TestModel.cs",
        Dictionary<string, string>? analyzerConfigOptions = null)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "SourceGeneratorTests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, path: fileName)],
            references: References,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var optionsProvider = analyzerConfigOptions is null
            ? null
            : new TestAnalyzerConfigOptionsProvider(analyzerConfigOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new AutoCodeGenerator().AsSourceGenerator()],
            additionalTexts: null,
            parseOptions: null,
            optionsProvider: optionsProvider);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        return driver.GetRunResult().Results[0];
    }

    public static ImmutableArray<Diagnostic> RunAnalyzer(string source, string fileName = "TestModel.cs")
    {
        var tree = CSharpSyntaxTree.ParseText(source, path: fileName);
        // Include generated attributes so AutoCode* names resolve for semantic model if needed;
        // analyzer itself is syntax-based.
        var compilation = CSharpCompilation.Create(
            "AnalyzerTests",
            [tree],
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new SuggestCrudModesAnalyzer());
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);
        return compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().Result;
    }

    public static string ApplyCodeFix(string source, string diagnosticId, string fileName = "TestModel.cs")
    {
        var workspace = new AdhocWorkspace();
        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);

        var solution = workspace.CurrentSolution
            .AddProject(projectId, "CodeFixTest", "CodeFixTest", LanguageNames.CSharp)
            .WithProjectCompilationOptions(
                projectId,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable))
            .WithProjectMetadataReferences(projectId, References)
            .AddDocument(documentId, fileName, source);

        workspace.TryApplyChanges(solution);
        var document = workspace.CurrentSolution.GetDocument(documentId)!;

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new SuggestCrudModesAnalyzer());
        var compilation = document.Project.GetCompilationAsync().Result!;
        var diagnostics = compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().Result
            .Where(d => d.Id == diagnosticId)
            .ToImmutableArray();

        Assert.NotEmpty(diagnostics);

        var provider = new SuggestCrudModesCodeFixProvider();
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(
            document,
            diagnostics[0],
            (action, _) => actions.Add(action),
            CancellationToken.None);
        provider.RegisterCodeFixesAsync(context).GetAwaiter().GetResult();
        Assert.NotEmpty(actions);

        var operations = actions[0].GetOperationsAsync(CancellationToken.None).Result;
        var apply = operations.OfType<ApplyChangesOperation>().Single();
        var newDoc = apply.ChangedSolution.GetDocument(documentId)!;
        return newDoc.GetTextAsync().Result.ToString();
    }

    public static GeneratorRunResult RunEntityFilesGenerator(
        string entityPath,
        string entityJson,
        string? source = null,
        Dictionary<string, string>? analyzerConfigOptions = null)
    {
        var trees = new List<SyntaxTree>();
        if (!string.IsNullOrWhiteSpace(source))
            trees.Add(CSharpSyntaxTree.ParseText(source));

        var compilation = CSharpCompilation.Create(
            assemblyName: "EntityFilesGeneratorTests",
            syntaxTrees: trees,
            references: References,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        var optionsProvider = analyzerConfigOptions is null
            ? null
            : new TestAnalyzerConfigOptionsProvider(analyzerConfigOptions);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new AutoFilesGenerator().AsSourceGenerator()],
            additionalTexts: ImmutableArray.Create<AdditionalText>(new InMemoryAdditionalText(entityPath, entityJson)),
            parseOptions: CSharpParseOptions.Default,
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        return driver.GetRunResult().Results[0];
    }

    public static string GenerateFromEntityJson(string entityPath, string entityJson)
    {
        var definition = EntityFileParser.Parse(entityJson);
        var model = EntityFileParser.ToGenerationResult(entityPath, definition);
        return SourceGeneratorHelper.GeneratorCode(model.Nullable, model.ClassInfos);
    }

    public static string GetAutoCodeAttributeSource()
    {
        var result = RunGenerator("namespace N { public interface IEmpty {} }");
        return GetGeneratedSource(result, "AutoCodeAttribute.g.cs");
    }

    public static void AssertEntityJsonCompiles(string entityPath, string entityJson, string? additionalSource = null)
    {
        var generated = GenerateFromEntityJson(entityPath, entityJson);
        var trees = new List<SyntaxTree> { CSharpSyntaxTree.ParseText(generated) };

        if (!string.IsNullOrWhiteSpace(additionalSource))
            trees.Add(CSharpSyntaxTree.ParseText(additionalSource));

        if (generated.Contains("InpcPropertyBindableBaseAbstract", StringComparison.Ordinal))
            trees.Add(CSharpSyntaxTree.ParseText(GetAutoCodeAttributeSource()));

        var compilation = CSharpCompilation.Create(
            "EntityGeneratedCodeValidation",
            trees,
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);
    }

    public static void AssertGeneratedCodeCompiles(GeneratorRunResult result, params string[] generatedFileSuffixes)
    {
        var attributeSource = TryGetGeneratedSource(result, "AutoCodeAttribute.g.cs");
        var trees = new List<SyntaxTree>();

        if (attributeSource is not null)
            trees.Add(CSharpSyntaxTree.ParseText(attributeSource));

        foreach (var suffix in generatedFileSuffixes)
            trees.Add(CSharpSyntaxTree.ParseText(GetGeneratedSource(result, suffix)));

        var compilation = CSharpCompilation.Create(
            "GeneratedCodeValidation",
            trees,
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);
    }

    public static string GetGeneratedSource(GeneratorRunResult result, string fileSuffix)
        => result.GeneratedSources
            .Single(s => s.HintName.EndsWith(fileSuffix, StringComparison.Ordinal))
            .SourceText
            .ToString();

    public static string? TryGetGeneratedSource(GeneratorRunResult result, string fileSuffix)
        => result.GeneratedSources
            .SingleOrDefault(s => s.HintName.EndsWith(fileSuffix, StringComparison.Ordinal))
            .SourceText
            ?.ToString();

    public static ImmutableArray<Diagnostic> GetGeneratorDiagnostics(GeneratorRunResult result)
        => result.Diagnostics;

    public static string GetGeneratedSourcesContaining(GeneratorRunResult result, string nameFragment)
    {
        var parts = result.GeneratedSources
            .Where(s => s.HintName.Contains(nameFragment, StringComparison.OrdinalIgnoreCase)
                        && !s.HintName.Equals("AutoCodeAttribute.g.cs", StringComparison.Ordinal))
            .Select(s => s.SourceText.ToString())
            .ToArray();
        Assert.NotEmpty(parts);
        return string.Join("\n", parts);
    }

    public static string NormalizeGeneratedCode(string source)
    {
        source = Regex.Replace(source, @"运行时版本:[^\r\n]*", "运行时版本:<normalized>");
        source = Regex.Replace(source, @"Version=[\d\.]+,", "Version=<normalized>,");
        return source;
    }
}
