using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using AutoModeCodeGenerator.Internal.CodeGeneration;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 与嵌入模板 <c>AutoCodeCrud</c> 常量对齐的 mode Id。
/// 生成器无法引用 PostInitialization 注入的类型，故在此镜像字符串。
/// </summary>
internal static class AutoCodeModeIds
{
    public const string Entity = "Entity";
    public const string Dto = "Dto";
    public const string Create = "CreateInput";
    public const string Update = "UpdateInput";
    public const string Detail = "DetailOutput";
    public const string Search = "SearchInput";
}

/// <summary>
/// 镜像嵌入模板中的 <c>AutoCodeModeFlags</c> / <c>AutoCodeCrudOmit</c> 位值（须保持一致）。
/// </summary>
internal static class AutoCodeModeFlagBits
{
    public const int Entity = 1;
    public const int Dto = 2;
    public const int Create = 4;
    public const int Update = 8;
    public const int Detail = 16;
    public const int Search = 32;
}

/// <summary>镜像嵌入模板 <c>AutoCodeMorphKind</c>（须保持一致）。</summary>
internal static class AutoCodeMorphKinds
{
    public const int File = 0;
    public const int SearchRange = 1;
    public const int StringEnum = 2;
    public const int StringArray = 3;
}

/// <summary>类模式中间模型（Extractor 与各 Expander 共享）。</summary>
internal sealed record ClassModeInfo(
    string Id,
    string Name,
    string? Prefix,
    string? Suffix,
    Accessibility? Modifier,
    bool IsPartial,
    ImmutableArray<string> Attributes,
    string? Remarks,
    string? Example,
    string? SummaryPrefix,
    string? SummarySuffix,
    string Summary,
    string? Inherit,
    ImmutableArray<string> Interfaces,
    ImmutableArray<INamedTypeSymbol> InterfaceSymbols,
    ImmutableArray<string> Usings,
    string? Namespace,
    string? NamespacePrefix,
    string? NamespaceSuffix,
    bool? IsAbstract,
    bool InheritAttribute,
    bool ToNullable,
    ClassStyleEnum ClassStyle);

/// <summary>属性绑定中间模型：一条源属性在若干 mode 上的投影。</summary>
internal sealed record PropertyBindingInfo(
    string SourceName,
    string Name,
    string? Prefix,
    string? Suffix,
    Accessibility? Modifier,
    ImmutableArray<string> Attributes,
    string? Remarks,
    string? Example,
    string? SummaryPrefix,
    string? SummarySuffix,
    string Summary,
    string? Id,
    ImmutableArray<string> Ids,
    string? DefaultValue,
    string? Type,
    bool? IsVirtual,
    bool? IsOverride,
    bool? IsNew,
    bool? IsNullable,
    ImmutableArray<string> InheritAttributes,
    bool? IsInit,
    bool? IsRequired)
{
    public bool MatchesMode(string modeId)
        => Id == modeId || Ids.Contains(modeId);

    public IEnumerable<string> AllModeIds()
    {
        if (!string.IsNullOrWhiteSpace(Id))
            yield return Id!;
        foreach (var id in Ids)
        {
            if (!string.IsNullOrWhiteSpace(id))
                yield return id;
        }
    }
}
