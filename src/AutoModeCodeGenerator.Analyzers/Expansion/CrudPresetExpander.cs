using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using static AutoModeCodeGenerator.Internal.AttributeArgumentReader;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 将 <c>AutoCodeCrudModes</c> / <c>AutoCodeRelation</c> 展开为与手写 ClassModes/Property 等价的内部模型。
/// </summary>
internal static class CrudPresetExpander
{
    internal const string AutoCodeCrudModesAttribute = "AutoCodeGenerator.AutoCodeCrudModesAttribute";
    internal const string AutoCodeRelationAttribute = "AutoCodeGenerator.AutoCodeRelationAttribute";

    public static ImmutableArray<ClassModeInfo> ExpandCrudModes(
        INamedTypeSymbol symbol,
        AttributeData crudAttr,
        Accessibility defaultModifier,
        string defaultNamespace,
        string summary,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var named = crudAttr.NamedArguments;
        var omit = GetValue<int?>(named, "OmitModes") ?? 0;
        var entitySuffix = GetValue<string>(named, "EntitySuffix") ?? "Record";
        var dtoSuffix = GetValue<string>(named, "DtoSuffix") ?? "Dto";
        var createSuffix = GetValue<string>(named, "CreateSuffix") ?? AutoCodeModeIds.Create;
        var updateSuffix = GetValue<string>(named, "UpdateSuffix") ?? AutoCodeModeIds.Update;
        var detailSuffix = GetValue<string>(named, "DetailSuffix") ?? AutoCodeModeIds.Detail;
        var searchSuffix = GetValue<string>(named, "SearchSuffix") ?? AutoCodeModeIds.Search;
        var ns = GetValue<string>(named, "Namespace") ?? defaultNamespace;
        var nsPrefix = GetValue<string>(named, "NamespacePrefix");
        var nsSuffix = GetValue<string>(named, "NamespaceSuffix");
        var searchToNullable = GetValue<bool?>(named, "SearchToNullable") ?? true;
        var inheritDto = GetValue<bool?>(named, "CreateUpdateDetailInheritDto") ?? true;
        var createSummary = GetValue<string>(named, "CreateSummaryPrefix") ?? "创建";
        var updateSummary = GetValue<string>(named, "UpdateSummaryPrefix") ?? "更新";
        var detailSummary = GetValue<string>(named, "DetailSummaryPrefix") ?? "详情";
        var searchSummary = GetValue<string>(named, "SearchSummaryPrefix") ?? "搜索";
        var entityInterfaces = ToImmutableStringArray(
            GetValues<string>(named, "EntityInterfaceTypes")
            ?? GetValues<string>(named, "EntityInterfaceStrs"));
        var entityInterfaceSymbols = GetTypeSymbols(named, "EntityInterfaceTypes");
        var entityAttributes = ToImmutableStringArray(GetValues<string>(named, "EntityAttributes"));

        var name = symbol.Name;
        var dtoTypeName = name + dtoSuffix;
        var builder = ImmutableArray.CreateBuilder<ClassModeInfo>();

        void Add(
            string id,
            string suffix,
            string? inherit,
            ImmutableArray<string> interfaces,
            ImmutableArray<INamedTypeSymbol> interfaceSymbols,
            ImmutableArray<string> attributes,
            bool toNullable,
            string? summaryPrefix)
        {
            builder.Add(new ClassModeInfo(
                Id: id,
                Name: name,
                Prefix: null,
                Suffix: suffix,
                Modifier: defaultModifier,
                IsPartial: true,
                Attributes: attributes,
                Remarks: null,
                Example: null,
                SummaryPrefix: summaryPrefix,
                SummarySuffix: null,
                Summary: summary,
                Inherit: inherit,
                Interfaces: interfaces,
                InterfaceSymbols: interfaceSymbols,
                Usings: ImmutableArray<string>.Empty,
                Namespace: ns,
                NamespacePrefix: nsPrefix,
                NamespaceSuffix: nsSuffix,
                IsAbstract: false,
                InheritAttribute: false,
                ToNullable: toNullable,
                ClassStyle: ClassStyleEnum.Poco));
        }

        var emptyIfaces = ImmutableArray<INamedTypeSymbol>.Empty;

        if ((omit & AutoCodeModeFlagBits.Dto) == 0)
            Add(AutoCodeModeIds.Dto, dtoSuffix, null, ImmutableArray<string>.Empty, emptyIfaces, ImmutableArray<string>.Empty, false, null);

        if ((omit & AutoCodeModeFlagBits.Entity) == 0)
            Add(AutoCodeModeIds.Entity, entitySuffix, null, entityInterfaces, entityInterfaceSymbols, entityAttributes, false, null);

        if ((omit & AutoCodeModeFlagBits.Create) == 0)
            Add(AutoCodeModeIds.Create, createSuffix, inheritDto ? dtoTypeName : null, ImmutableArray<string>.Empty, emptyIfaces, ImmutableArray<string>.Empty, false, createSummary);

        if ((omit & AutoCodeModeFlagBits.Update) == 0)
            Add(AutoCodeModeIds.Update, updateSuffix, inheritDto ? dtoTypeName : null, ImmutableArray<string>.Empty, emptyIfaces, ImmutableArray<string>.Empty, false, updateSummary);

        if ((omit & AutoCodeModeFlagBits.Detail) == 0)
            Add(AutoCodeModeIds.Detail, detailSuffix, inheritDto ? dtoTypeName : null, ImmutableArray<string>.Empty, emptyIfaces, ImmutableArray<string>.Empty, false, detailSummary);

        if ((omit & AutoCodeModeFlagBits.Search) == 0)
            Add(AutoCodeModeIds.Search, searchSuffix, null, ImmutableArray<string>.Empty, emptyIfaces, ImmutableArray<string>.Empty, searchToNullable, searchSummary);

        return builder.ToImmutable();
    }

    public static ImmutableArray<ClassModeInfo> MergeModes(
        ImmutableArray<ClassModeInfo> presetModes,
        ImmutableArray<ClassModeInfo> handwrittenModes)
    {
        var map = new Dictionary<string, ClassModeInfo>(StringComparer.Ordinal);
        foreach (var mode in presetModes)
            map[mode.Id] = mode;
        foreach (var mode in handwrittenModes)
            map[mode.Id] = mode; // handwritten wins
        return map.Values.ToImmutableArray();
    }

    public static void ExpandRelations(
        INamedTypeSymbol symbol,
        ImmutableArray<ClassModeInfo> classModes,
        ImmutableArray<PropertyBindingInfo>.Builder result,
        Location location,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        var entitySuffix = classModes.FirstOrDefault(m => m.Id == AutoCodeModeIds.Entity)?.Suffix ?? "Record";
        var detailSuffix = classModes.FirstOrDefault(m => m.Id == AutoCodeModeIds.Detail)?.Suffix ?? AutoCodeModeIds.Detail;
        var searchSuffix = classModes.FirstOrDefault(m => m.Id == AutoCodeModeIds.Search)?.Suffix ?? AutoCodeModeIds.Search;

        foreach (var property in symbol.GetMembers().OfType<IPropertySymbol>()
                     .Concat(symbol.AllInterfaces.SelectMany(i => i.GetMembers().OfType<IPropertySymbol>())))
        {
            foreach (var attribute in property.GetAttributes().Where(
                f => f.AttributeClass?.ToDisplayString() == AutoCodeRelationAttribute))
            {
                var named = attribute.NamedArguments;
                var targetName = GetValue<string>(named, "TargetName");
                if (string.IsNullOrWhiteSpace(targetName))
                {
                    diagnostics.Add(GeneratorDiagnostics.Create(
                        GeneratorDiagnostics.RelationMissingTargetName,
                        location,
                        symbol.ToDisplayString(),
                        property.Name));
                    continue;
                }

                var includeNavigate = GetValue<bool?>(named, "IncludeNavigate") ?? true;
                var idSuffix = GetValue<string>(named, "IdSuffix") ?? "Id";
                var navigateTemplate = GetValue<string>(named, "NavigateAttribute")
                    ?? "global::SqlSugar.Navigate(global::SqlSugar.NavigateType.ManyToOne, nameof({0}Id))";
                var sourceType = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)?.TrimEnd('?');

                // Entity+Dto+Search with Id suffix
                result.Add(CreateBinding(
                    property,
                    name: property.Name,
                    suffix: idSuffix,
                    ids: ImmutableArray.Create(AutoCodeModeIds.Entity, AutoCodeModeIds.Dto, AutoCodeModeIds.Search),
                    type: sourceType,
                    isNullable: property.NullableAnnotation == NullableAnnotation.Annotated,
                    attributes: ImmutableArray<string>.Empty));

                // Detail navigation
                result.Add(CreateBinding(
                    property,
                    name: property.Name,
                    suffix: null,
                    ids: ImmutableArray.Create(AutoCodeModeIds.Detail),
                    type: targetName + detailSuffix,
                    isNullable: true,
                    attributes: ImmutableArray<string>.Empty));

                // Search shape
                result.Add(CreateBinding(
                    property,
                    name: property.Name,
                    suffix: null,
                    ids: ImmutableArray.Create(AutoCodeModeIds.Search),
                    type: targetName + searchSuffix,
                    isNullable: true,
                    attributes: ImmutableArray<string>.Empty));

                if (includeNavigate)
                {
                    var navigateAttr = string.Format(navigateTemplate, property.Name);
                    result.Add(CreateBinding(
                        property,
                        name: property.Name,
                        suffix: null,
                        ids: ImmutableArray.Create(AutoCodeModeIds.Entity),
                        type: targetName + entitySuffix,
                        isNullable: true,
                        attributes: ImmutableArray.Create(navigateAttr)));
                }
            }
        }
    }

    public static bool IsValidTypeName(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return false;

        var trimmed = typeName!.Trim();
        if (trimmed.Contains(' '))
            return false;

        // Allow namespace.Type, Type<T>, global::Type
        var withoutGlobal = StripGlobal(trimmed);

        foreach (var part in withoutGlobal.Split('.'))
        {
            var identifier = part ?? string.Empty;
            var genericIndex = identifier.IndexOf('<');
            if (genericIndex >= 0)
                identifier = identifier.Substring(0, genericIndex);

            if (identifier.Length == 0 || !SyntaxFacts.IsValidIdentifier(identifier))
                return false;
        }

        return true;
    }

    private static PropertyBindingInfo CreateBinding(
        IPropertySymbol property,
        string name,
        string? suffix,
        ImmutableArray<string> ids,
        string? type,
        bool isNullable,
        ImmutableArray<string> attributes)
        => new(
            SourceName: property.Name,
            Name: name,
            Prefix: null,
            Suffix: suffix,
            Modifier: property.DeclaredAccessibility,
            Attributes: attributes,
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
}
