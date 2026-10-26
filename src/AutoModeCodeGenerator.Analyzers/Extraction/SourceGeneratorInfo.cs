using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// Emit 中间模型：属性绑定（特性 Extract / entity.json 解析最终都落到此结构）。
/// </summary>
public sealed record SourceGeneratorPropertyInfo(
    string? Name,
    Accessibility? Modifier,
    string? Summary,
    string? Remarks,
    string? Example,
    bool? IsVirtual,
    bool? IsOverride,
    bool? IsNew,
    string? Type,
    string? DefaultValue,
    bool? IsNullable,
    ImmutableArray<string> Attributes,
    ImmutableArray<string> InheritAttributes,
    bool IsInit = false,
    bool IsRequired = false);

/// <summary>
/// Emit 中间模型：单个生成类型（Poco / Record / Inpc 发射器的输入）。
/// </summary>
public sealed record SourceGeneratorClassInfo(
    string? Name,
    Accessibility? Modifier,
    string? Summary,
    string? Remarks,
    string? Example,
    string? ClassNamespace,
    ImmutableArray<string> Usings,
    ImmutableArray<string> Interfaces,
    bool? IsAbstract,
    string? Inherit,
    ImmutableArray<SourceGeneratorPropertyInfo> Properties,
    ImmutableArray<string> Attributes,
    bool? IsPartial,
    bool InheritAttribute,
    bool ToNullable,
    ClassStyleEnum ClassStyle);

public sealed record GenerationResult(
    string FileName,
    NullableEnum? Nullable,
    ImmutableArray<SourceGeneratorClassInfo> ClassInfos,
    ImmutableArray<Diagnostic> Diagnostics);

public enum NullableEnum
{
    Disable,
    Enable,
    Restore,
    DisableAnnotations,
    EnableAnnotations,
    RestoreAnnotations,
    DisableWarnings,
    EnableWarnings,
    RestoreWarnings
}

public enum ClassStyleEnum
{
    Poco,
    Record,
    Inpc
}
