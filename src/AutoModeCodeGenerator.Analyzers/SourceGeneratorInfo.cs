using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace 自动代码生成;

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
    ImmutableArray<string> InheritAttributes);

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
