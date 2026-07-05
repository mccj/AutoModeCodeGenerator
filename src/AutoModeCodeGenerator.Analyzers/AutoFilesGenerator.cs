using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using 自动代码生成;
using 自动代码生成.EntityFiles;

/// <summary>
/// 从 AdditionalFiles 中的 <c>.entity.json</c> 文件增量生成 POCO / Record / Inpc 类。
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class AutoFilesGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 仅处理以 .entity.json 结尾的附加文件。
        var entityFiles = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".entity.json", StringComparison.OrdinalIgnoreCase));

        context.RegisterSourceOutput(entityFiles, static (ctx, file) =>
        {
            var sourceText = file.GetText(ctx.CancellationToken);
            if (sourceText is null)
                return;

            var fileName = Path.GetFileName(file.Path);

            try
            {
                var definition = EntityFileParser.Parse(sourceText.ToString());
                var model = EntityFileParser.ToGenerationResult(file.Path, definition, sourceText);

                foreach (var diagnostic in model.Diagnostics)
                    ctx.ReportDiagnostic(diagnostic);

                if (model.ClassInfos.IsDefaultOrEmpty)
                    return;

                var sourceCode = SourceGeneratorHelper.GeneratorCode(model.Nullable, model.ClassInfos);
                ctx.AddSource($"{model.FileName}.entity.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
            }
            catch (EntityFileParseException ex)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.EntityFileGenerationFailed,
                    EntityFileLocations.Create(file.Path, sourceText, ex.Line, ex.Column),
                    fileName,
                    ex.Message));
            }
            catch (Exception ex)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(
                    GeneratorDiagnostics.EntityFileGenerationFailed,
                    EntityFileLocations.Create(file.Path, sourceText, 0, 0),
                    fileName,
                    ex.Message));
            }
        });
    }
}
