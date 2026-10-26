using Microsoft.CodeAnalysis.Diagnostics;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// MSBuild / AnalyzerConfig 可读选项（特性生成与 entity.json 共用同一入口）。
/// </summary>
internal sealed record AutoModeGeneratorOptions(
    string SearchRangeTemplate,
    bool GenerateEntityFiles,
    string EntityFileSuffix)
{
    public const string DefaultSearchRangeTemplate = "SearchRange<{0}>";
    public const string DefaultEntityFileSuffix = ".entity.json";

    private const string SearchRangeProperty = "build_property.AutoModeSearchRangeTemplate";
    private const string GenerateEntityFilesProperty = "build_property.AutoModeGenerateEntityFiles";
    private const string EntityFileSuffixProperty = "build_property.AutoModeEntityFileSuffix";

    public static AutoModeGeneratorOptions Default { get; }
        = new(DefaultSearchRangeTemplate, GenerateEntityFiles: true, DefaultEntityFileSuffix);

    public static AutoModeGeneratorOptions From(AnalyzerConfigOptions globalOptions)
    {
        var template = DefaultSearchRangeTemplate;
        if (globalOptions.TryGetValue(SearchRangeProperty, out var searchValue)
            && !string.IsNullOrWhiteSpace(searchValue))
        {
            template = searchValue.Trim();
        }

        var generateEntityFiles = true;
        if (globalOptions.TryGetValue(GenerateEntityFilesProperty, out var generateValue)
            && bool.TryParse(generateValue, out var parsedGenerate))
        {
            generateEntityFiles = parsedGenerate;
        }

        var entitySuffix = DefaultEntityFileSuffix;
        if (globalOptions.TryGetValue(EntityFileSuffixProperty, out var suffixValue)
            && !string.IsNullOrWhiteSpace(suffixValue))
        {
            entitySuffix = suffixValue.Trim();
        }

        return new AutoModeGeneratorOptions(template, generateEntityFiles, entitySuffix);
    }
}
