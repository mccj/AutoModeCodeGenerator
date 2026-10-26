using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using static AutoModeCodeGenerator.Internal.AttributeArgumentReader;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 在 <c>InjectMarkerMembers</c> 开启时，为 InterfaceTypes 尚未投影的成员补默认绑定。
/// 投影到 Entity + Dto（Detail 默认继承 Dto，避免 CS0108）以及 Search；
/// DateTime / 数值等类型在 Search 上使用区间模板。
/// </summary>
internal static class MarkerMemberInjector
{
    public static void Inject(
        ClassModeInfo[] classModes,
        ImmutableArray<PropertyBindingInfo>.Builder result,
        string searchRangeTemplate)
    {
        var entityNames = new HashSet<string>(
            result.Where(b => b.MatchesMode(AutoCodeModeIds.Entity))
                .Select(b => b.Prefix + b.Name + b.Suffix),
            StringComparer.Ordinal);

        foreach (var mode in classModes)
        {
            if (mode.InterfaceSymbols.IsDefaultOrEmpty)
                continue;

            foreach (var iface in mode.InterfaceSymbols)
            {
                foreach (var prop in InterfaceContractChecker.EnumerateInterfaceProperties(iface))
                {
                    if (entityNames.Contains(prop.Name))
                        continue;

                    var type = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)?.TrimEnd('?')
                        ?? "object";
                    var nullable = prop.NullableAnnotation == NullableAnnotation.Annotated
                        || prop.Type.IsReferenceType
                        || prop.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

                    // Detail 默认继承 Dto：同类型只投影 Entity+Dto，避免 CS0108。
                    result.Add(Binding(prop, ImmutableArray.Create(AutoCodeModeIds.Entity, AutoCodeModeIds.Dto), type, nullable));

                    if (IsRangeCandidate(prop.Type))
                    {
                        var element = UnwrapNullable(prop.Type)
                            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                            ?.TrimEnd('?')
                            ?? type;
                        element = StripGlobal(element);
                        var rangeType = string.Format(searchRangeTemplate, element);
                        result.Add(Binding(prop, ImmutableArray.Create(AutoCodeModeIds.Search), rangeType, isNullable: true));
                    }
                    else
                    {
                        result.Add(Binding(prop, ImmutableArray.Create(AutoCodeModeIds.Search), type, nullable));
                    }

                    entityNames.Add(prop.Name);
                }
            }
        }
    }

    private static PropertyBindingInfo Binding(
        IPropertySymbol property,
        ImmutableArray<string> ids,
        string type,
        bool isNullable)
        => new(
            SourceName: property.Name,
            Name: property.Name,
            Prefix: null,
            Suffix: null,
            Modifier: Accessibility.Public,
            Attributes: ImmutableArray<string>.Empty,
            Remarks: null,
            Example: null,
            SummaryPrefix: null,
            SummarySuffix: null,
            Summary: property.Name,
            Id: null,
            Ids: ids,
            DefaultValue: null,
            Type: type,
            IsVirtual: null,
            IsOverride: null,
            IsNew: null,
            IsNullable: isNullable,
            InheritAttributes: ImmutableArray<string>.Empty,
            IsInit: null,
            IsRequired: null);

    private static bool IsRangeCandidate(ITypeSymbol type)
    {
        var t = UnwrapNullable(type);
        return t.SpecialType is SpecialType.System_DateTime
            or SpecialType.System_Int32
            or SpecialType.System_Int64
            or SpecialType.System_Decimal
            or SpecialType.System_Double
            or SpecialType.System_Single
            || t.Name is "DateTimeOffset" or "DateOnly" or "TimeOnly";
    }

    private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
        => type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && type is INamedTypeSymbol { TypeArguments.Length: > 0 } n
            ? n.TypeArguments[0]
            : type;
}
