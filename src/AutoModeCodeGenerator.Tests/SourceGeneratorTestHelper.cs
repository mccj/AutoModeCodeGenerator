using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using 自动代码生成;
using 自动代码生成.EntityFiles;

namespace AutoModeCodeGenerator.Tests;

internal sealed class InMemoryAdditionalText(string path, string content) : AdditionalText
{
    private readonly SourceText _sourceText = SourceText.From(content, Encoding.UTF8);

    public override string Path { get; } = path;

    public override SourceText GetText(CancellationToken cancellationToken = default) => _sourceText;
}

internal static class SourceGeneratorTestHelper
{
    private static readonly MetadataReference[] References =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.ComponentModel.DisplayNameAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute).Assembly.Location),
    ];

    public static MetadataReference[] GetMetadataReferences() => References;

    public static GeneratorRunResult RunGenerator(string source, string fileName = "TestModel.cs")
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "SourceGeneratorTests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, path: fileName)],
            references: References,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new AutoCodeGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        return driver.GetRunResult().Results[0];
    }

    public static GeneratorRunResult RunEntityFilesGenerator(string entityPath, string entityJson, string? source = null)
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

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            [new AutoFilesGenerator().AsSourceGenerator()],
            additionalTexts: ImmutableArray.Create<AdditionalText>(new InMemoryAdditionalText(entityPath, entityJson)),
            parseOptions: CSharpParseOptions.Default);

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

    public static string NormalizeGeneratedCode(string source)
    {
        source = Regex.Replace(source, @"运行时版本:[^\r\n]*", "运行时版本:<normalized>");
        source = Regex.Replace(source, @"Version=[\d\.]+,", "Version=<normalized>,");
        return source;
    }
}
