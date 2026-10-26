using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using static AutoModeCodeGenerator.Internal.AttributeArgumentReader;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 将 <c>AutoCodeMorph</c> 展开为按 mode 区分类型/特性的属性绑定。
/// </summary>
internal static class MorphExpander
{
    internal const string AutoCodeMorphAttribute = "AutoCodeGenerator.AutoCodeMorphAttribute";

    public static void ExpandMorphs(
        INamedTypeSymbol symbol,
        ImmutableArray<PropertyBindingInfo>.Builder result,
        string defaultSearchRangeTemplate,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        foreach (var property in symbol.GetMembers().OfType<IPropertySymbol>()
                     .Concat(symbol.AllInterfaces.SelectMany(i => i.GetMembers().OfType<IPropertySymbol>())))
        {
            foreach (var attribute in property.GetAttributes().Where(
                f => f.AttributeClass?.ToDisplayString() == AutoCodeMorphAttribute))
            {
                ExpandOne(property, attribute.NamedArguments, result, defaultSearchRangeTemplate);
            }
        }
    }

    private static void ExpandOne(
        IPropertySymbol property,
        ImmutableArray<KeyValuePair<string, TypedConstant>> named,
        ImmutableArray<PropertyBindingInfo>.Builder result,
        string defaultSearchRangeTemplate)
    {
        var kind = GetValue<int?>(named, "Kind") ?? AutoCodeMorphKinds.File;
        var sourceType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)?.TrimEnd('?')
            ?? "object";
        var entityAttrs = ToImmutableStringArray(TryGetValues<string>(named, "EntityAttributes"));
        var apiAttrs = ToImmutableStringArray(TryGetValues<string>(named, "ApiAttributes"));

        switch (kind)
        {
            case AutoCodeMorphKinds.File:
            {
                // Create/Update 默认继承 Dto，只在 Dto 投影；Detail 类型不同时单独投影。
                var entityType = GetValue<string>(named, "EntityType") ?? "string";
                var createUpdate = GetValue<string>(named, "CreateUpdateType") ?? "FileCreateInput";
                var detail = GetValue<string>(named, "DetailType") ?? "FileOutput";

                Add(result, property, AutoCodeModeIds.Entity, entityType, entityAttrs);
                Add(result, property, AutoCodeModeIds.Dto, createUpdate, apiAttrs);
                if (!string.Equals(detail, createUpdate, StringComparison.Ordinal))
                    Add(result, property, AutoCodeModeIds.Detail, detail, apiAttrs);
                break;
            }
            case AutoCodeMorphKinds.SearchRange:
            {
                var template = ResolveSearchRangeTemplate(
                    named,
                    stringKey: "RangeTypeTemplate",
                    typeKey: "RangeType",
                    fallback: defaultSearchRangeTemplate);
                var searchType = GetValue<string>(named, "SearchType")
                    ?? string.Format(template, StripGlobal(sourceType));
                Add(result, property, AutoCodeModeIds.Search, searchType, apiAttrs);
                break;
            }
            case AutoCodeMorphKinds.StringEnum:
            {
                // 生产常见：各 mode 仍为枚举类型，仅 Entity/Api 特性宏不同。
                // EntityType 默认保留枚举；若需 DB 列存 string，显式 EntityType = "string"。
                var enumType = GetValue<string>(named, "EnumType")
                    ?? GetValue<string>(named, "CreateUpdateType")
                    ?? StripGlobal(sourceType);
                var entityType = GetValue<string>(named, "EntityType") ?? enumType;

                Add(result, property, AutoCodeModeIds.Entity, entityType, entityAttrs);
                Add(result, property, AutoCodeModeIds.Dto, enumType, apiAttrs);
                Add(result, property, AutoCodeModeIds.Search, enumType, apiAttrs);
                break;
            }
            case AutoCodeMorphKinds.StringArray:
            {
                var entityType = GetValue<string>(named, "EntityType") ?? "string";
                var arrayType = GetValue<string>(named, "CreateUpdateType") ?? "string[]";
                var detail = GetValue<string>(named, "DetailType") ?? arrayType;

                Add(result, property, AutoCodeModeIds.Entity, entityType, entityAttrs);
                Add(result, property, AutoCodeModeIds.Dto, arrayType, apiAttrs);
                if (!string.Equals(detail, arrayType, StringComparison.Ordinal))
                    Add(result, property, AutoCodeModeIds.Detail, detail, apiAttrs);
                break;
            }
        }
    }

    private static void Add(
        ImmutableArray<PropertyBindingInfo>.Builder result,
        IPropertySymbol property,
        string modeId,
        string type,
        ImmutableArray<string> attributes)
        => result.Add(new PropertyBindingInfo(
            SourceName: property.Name,
            Name: property.Name,
            Prefix: null,
            Suffix: null,
            Modifier: property.DeclaredAccessibility,
            Attributes: attributes,
            Remarks: null,
            Example: null,
            SummaryPrefix: null,
            SummarySuffix: null,
            Summary: property.Name,
            Id: null,
            Ids: ImmutableArray.Create(modeId),
            DefaultValue: null,
            Type: type,
            IsVirtual: null,
            IsOverride: null,
            IsNew: null,
            IsNullable: property.NullableAnnotation == NullableAnnotation.Annotated,
            InheritAttributes: ImmutableArray<string>.Empty,
            IsInit: null,
            IsRequired: null));
}
