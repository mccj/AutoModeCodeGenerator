using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using AutoModeCodeGenerator.Internal;
using AutoModeCodeGenerator.Internal.EntityFiles;

/// <summary>
/// 双主入口之一：从 AdditionalFiles 中的 <c>.entity.json</c> 声明式生成类型。
/// 解析后复用与特性路径相同的 Emit（<see cref="SourceGeneratorHelper"/>）；
/// 当前不经 CrudModes / Relation / Morph 展开器（能力边界见 docs/ARCHITECTURE.md）。
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class AutoFilesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider
            .Select(static (provider, _) => AutoModeGeneratorOptions.From(provider.GlobalOptions));

        var entityFiles = context.AdditionalTextsProvider
            .Combine(options)
            .Where(static pair =>
            {
                var (file, opts) = pair;
                if (!opts.GenerateEntityFiles)
                    return false;

                return file.Path.EndsWith(opts.EntityFileSuffix, StringComparison.OrdinalIgnoreCase);
            })
            .Select(static (pair, cancellationToken) =>
            {
                var (file, _) = pair;
                var sourceText = file.GetText(cancellationToken);
                return new EntityFileInput(file.Path, sourceText?.ToString());
            })
            .WithComparer(EntityFileInputComparer.Instance)
            .Select(static (input, _) => ParseEntityFile(input))
            .WithTrackingName("AutoFiles.ParseEntity");

        context.RegisterSourceOutput(entityFiles, static (ctx, parsed) =>
        {
            foreach (var diagnostic in parsed.Diagnostics)
                ctx.ReportDiagnostic(diagnostic);

            if (parsed.SourceCode is null || parsed.HintName is null)
                return;

            ctx.AddSource(parsed.HintName, SourceText.From(parsed.SourceCode, Encoding.UTF8));
        });
    }

    private static ParsedEntityFile ParseEntityFile(EntityFileInput input)
    {
        if (input.Content is null)
        {
            return new ParsedEntityFile(
                HintName: null,
                SourceCode: null,
                Diagnostics: ImmutableArray<Diagnostic>.Empty);
        }

        var sourceText = SourceText.From(input.Content, Encoding.UTF8);
        var fileName = Path.GetFileName(input.Path);

        try
        {
            var definition = EntityFileParser.Parse(input.Content);
            var model = EntityFileParser.ToGenerationResult(input.Path, definition, sourceText);

            if (model.ClassInfos.IsDefaultOrEmpty)
            {
                return new ParsedEntityFile(
                    HintName: null,
                    SourceCode: null,
                    Diagnostics: model.Diagnostics);
            }

            var sourceCode = SourceGeneratorHelper.GeneratorCode(model.Nullable, model.ClassInfos);
            return new ParsedEntityFile(
                HintName: $"{model.FileName}.entity.g.cs",
                SourceCode: sourceCode,
                Diagnostics: model.Diagnostics);
        }
        catch (EntityFileParseException ex)
        {
            return new ParsedEntityFile(
                HintName: null,
                SourceCode: null,
                Diagnostics: ImmutableArray.Create(Diagnostic.Create(
                    GeneratorDiagnostics.EntityFileGenerationFailed,
                    EntityFileLocations.Create(input.Path, sourceText, ex.Line, ex.Column),
                    fileName,
                    ex.Message)));
        }
        catch (Exception ex)
        {
            return new ParsedEntityFile(
                HintName: null,
                SourceCode: null,
                Diagnostics: ImmutableArray.Create(Diagnostic.Create(
                    GeneratorDiagnostics.EntityFileGenerationFailed,
                    EntityFileLocations.Create(input.Path, sourceText, 0, 0),
                    fileName,
                    ex.Message)));
        }
    }

    private sealed record EntityFileInput(string Path, string? Content);

    private sealed record ParsedEntityFile(
        string? HintName,
        string? SourceCode,
        ImmutableArray<Diagnostic> Diagnostics);

    private sealed class EntityFileInputComparer : IEqualityComparer<EntityFileInput>
    {
        public static readonly EntityFileInputComparer Instance = new();

        public bool Equals(EntityFileInput? x, EntityFileInput? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return string.Equals(x.Path, y.Path, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Content, y.Content, StringComparison.Ordinal);
        }

        public int GetHashCode(EntityFileInput obj)
        {
            unchecked
            {
                var hash = StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Path);
                hash = (hash * 397) ^ (obj.Content is null ? 0 : StringComparer.Ordinal.GetHashCode(obj.Content));
                return hash;
            }
        }
    }
}
