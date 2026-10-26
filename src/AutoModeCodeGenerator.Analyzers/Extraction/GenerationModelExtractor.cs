using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using AutoModeCodeGenerator.Internal.CodeGeneration;

using static AutoModeCodeGenerator.Internal.AttributeArgumentReader;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 从语义模型提取生成配置：解析特性 → 合并 CRUD 预设 → 展开 Relation/Morph/Column →
/// （可选）注入 marker 成员 → 组装 <see cref="SourceGeneratorClassInfo"/>。
/// </summary>
internal static class GenerationModelExtractor
{
    internal const string AutoCodeClassModesAttribute = "AutoCodeGenerator.AutoCodeClassModesAttribute";
    internal const string AutoCodePropertyAttribute = "AutoCodeGenerator.AutoCodePropertyAttribute";
    internal const string AutoCodeNullableAttribute = "AutoCodeGenerator.AutoCodeNullableAttribute";
    internal const string CustomAttributeAttribute = "AutoCodeGenerator.CustomAttributeAttribute";
    internal const string DefaultInpcInherit = "global::AutoCodeGenerator.InpcPropertyBindableBaseAbstract";

    public static bool HasGenerationAttributes(INamedTypeSymbol symbol)
    {
        static bool IsGen(AttributeData a)
        {
            var name = a.AttributeClass?.ToDisplayString();
            return name is AutoCodeClassModesAttribute
                or CrudPresetExpander.AutoCodeCrudModesAttribute;
        }

        return symbol.GetAttributes().Any(IsGen)
            || symbol.AllInterfaces.SelectMany(i => i.GetAttributes()).Any(IsGen);
    }

    public static GenerationResult Extract(
        INamedTypeSymbol symbol,
        Location location,
        AutoModeGeneratorOptions options,
        CancellationToken cancellationToken)
    {
        var fileName = GetFileName(symbol);
        try
        {
            var (nullable, classInfos, diagnostics) = BuildModel(symbol, location, options, cancellationToken);
            return new GenerationResult(fileName, nullable, classInfos, diagnostics);
        }
        catch (Exception ex)
        {
            return new GenerationResult(
                fileName,
                Nullable: null,
                ClassInfos: ImmutableArray<SourceGeneratorClassInfo>.Empty,
                Diagnostics: ImmutableArray.Create(
                    GeneratorDiagnostics.Create(
                        GeneratorDiagnostics.GenerationFailed,
                        location,
                        symbol.ToDisplayString(),
                        ex.Message)));
        }
    }

    private static string GetFileName(INamedTypeSymbol symbol)
        => $"{(symbol.ContainingNamespace?.IsGlobalNamespace == true ? "_" : symbol.ContainingNamespace?.ToDisplayString())}.{symbol.Name}";

    private static (NullableEnum? nullable, ImmutableArray<SourceGeneratorClassInfo> classInfos, ImmutableArray<Diagnostic> diagnostics) BuildModel(
        INamedTypeSymbol symbol,
        Location location,
        AutoModeGeneratorOptions options,
        CancellationToken cancellationToken)
    {
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var attributeDatas = symbol.GetAttributes()
            .Concat(symbol.AllInterfaces.SelectMany(i => i.GetAttributes()))
            .ToArray();

        var autoCodeNullableAttribute = attributeDatas.FirstOrDefault(
            f => f.AttributeClass?.ToDisplayString() == AutoCodeNullableAttribute);
        var autoCodeNullable = GetValue<NullableEnum?>(autoCodeNullableAttribute?.NamedArguments, "Type")
            ?? NullableEnum.Restore;

        var defaultNamespace = symbol.ContainingNamespace?.IsGlobalNamespace == true
            ? "AutoGenerator"
            : symbol.ContainingNamespace?.ToDisplayString() + ".AutoGenerator";
        var typeSummary = GetDocSummary(symbol.GetDocumentationCommentXml(cancellationToken: cancellationToken))
            ?? symbol.Name;

        // --- ClassModes（手写，1.0.1 起 Obsolete；仍为一等解析路径）---
        var rawClassModes = attributeDatas
            .Where(f => f.AttributeClass?.ToDisplayString() == AutoCodeClassModesAttribute)
            .Select(mode => new { mode.NamedArguments, Id = GetValue<string>(mode.NamedArguments, "Id") })
            .ToArray();

        foreach (var mode in rawClassModes.Where(m => string.IsNullOrWhiteSpace(m.Id)))
        {
            diagnostics.Add(GeneratorDiagnostics.Create(
                GeneratorDiagnostics.MissingClassModeId,
                location,
                symbol.ToDisplayString()));
        }

        var handwrittenModes = rawClassModes
            .Where(mode => !string.IsNullOrWhiteSpace(mode.Id))
            .Select(mode =>
            {
                var classStyle = GetValue<ClassStyleEnum?>(mode.NamedArguments, "ClassStyle") ?? ClassStyleEnum.Poco;
                var inherit = GetValue<string>(mode.NamedArguments, "InheritType")
                    ?? GetValue<string>(mode.NamedArguments, "InheritStr");
                if (classStyle == ClassStyleEnum.Inpc && string.IsNullOrWhiteSpace(inherit))
                    inherit = DefaultInpcInherit;

                var inheritDto = GetValue<bool?>(mode.NamedArguments, "InheritDto") == true;
                if (inheritDto && string.IsNullOrWhiteSpace(inherit))
                {
                    var dtoSuffix = GetValue<string>(mode.NamedArguments, "InheritDtoSuffix") ?? "Dto";
                    var modeName = GetValue<string>(mode.NamedArguments, "Name") ?? symbol.Name;
                    inherit = modeName + dtoSuffix;
                }

                return new ClassModeInfo(
                    Id: mode.Id!,
                    Name: GetValue<string>(mode.NamedArguments, "Name") ?? symbol.Name,
                    Prefix: GetValue<string>(mode.NamedArguments, "Prefix"),
                    Suffix: GetValue<string>(mode.NamedArguments, "Suffix"),
                    Modifier: GetValue<Accessibility?>(mode.NamedArguments, "Modifier") ?? symbol.DeclaredAccessibility,
                    IsPartial: GetValue<bool?>(mode.NamedArguments, "IsPartial") ?? true,
                    Attributes: ToImmutableStringArray(
                        (GetValues<string>(mode.NamedArguments, "Attributes") ?? [])
                        .Concat(attributeDatas
                            .Where(ff => ff.AttributeClass?.ToDisplayString() == CustomAttributeAttribute
                                && (GetValue<string>(ff.NamedArguments, "Id") == mode.Id
                                    || (GetValues<string>(ff.NamedArguments, "Ids") ?? []).Contains(mode.Id)))
                            .SelectMany(ff => GetValues<string>(ff.NamedArguments, "Attributes") ?? []))),
                    Remarks: GetValue<string>(mode.NamedArguments, "Remarks"),
                    Example: GetValue<string>(mode.NamedArguments, "Example"),
                    SummaryPrefix: GetValue<string>(mode.NamedArguments, "SummaryPrefix"),
                    SummarySuffix: GetValue<string>(mode.NamedArguments, "SummarySuffix"),
                    Summary: GetValue<string>(mode.NamedArguments, "Summary") ?? typeSummary,
                    Inherit: inherit,
                    Interfaces: ToImmutableStringArray(
                        GetValues<string>(mode.NamedArguments, "InterfaceTypes")
                        ?? GetValues<string>(mode.NamedArguments, "InterfaceStrs")),
                    InterfaceSymbols: GetTypeSymbols(mode.NamedArguments, "InterfaceTypes"),
                    Usings: ToImmutableStringArray(GetValues<string>(mode.NamedArguments, "Usings")),
                    Namespace: GetValue<string>(mode.NamedArguments, "Namespace") ?? defaultNamespace,
                    NamespacePrefix: GetValue<string>(mode.NamedArguments, "NamespacePrefix"),
                    NamespaceSuffix: GetValue<string>(mode.NamedArguments, "NamespaceSuffix"),
                    IsAbstract: GetValue<bool?>(mode.NamedArguments, "IsAbstract"),
                    InheritAttribute: GetValue<bool?>(mode.NamedArguments, "InheritAttribute") ?? false,
                    ToNullable: GetValue<bool?>(mode.NamedArguments, "ToNullable") ?? false,
                    ClassStyle: classStyle);
            })
            .ToImmutableArray();

        // --- CrudModes 预设展开，再与手写 Merge（同 Id 手写优先）---
        var crudAttr = attributeDatas.FirstOrDefault(
            f => f.AttributeClass?.ToDisplayString() == CrudPresetExpander.AutoCodeCrudModesAttribute);
        var presetModes = crudAttr is null
            ? ImmutableArray<ClassModeInfo>.Empty
            : CrudPresetExpander.ExpandCrudModes(
                symbol,
                crudAttr,
                symbol.DeclaredAccessibility,
                defaultNamespace,
                typeSummary,
                cancellationToken);

        var classModes = CrudPresetExpander.MergeModes(presetModes, handwrittenModes).ToArray();

        // Attach AutoCodeIndex attributes to Entity mode.
        var indexAttrs = ColumnIndexExpander.ExpandIndexes(symbol, out var indexColumnRefs);
        if (indexAttrs.Length > 0)
        {
            for (var i = 0; i < classModes.Length; i++)
            {
                if (classModes[i].Id != AutoCodeModeIds.Entity)
                    continue;
                classModes[i] = classModes[i] with
                {
                    Attributes = classModes[i].Attributes.AddRange(indexAttrs)
                };
            }
        }

        var injectMarkerMembers =
            GetValue<bool?>(crudAttr?.NamedArguments, "InjectMarkerMembers") == true
            || attributeDatas.Any(a =>
                a.AttributeClass?.ToDisplayString() == AutoCodeClassModesAttribute
                && GetValue<bool?>(a.NamedArguments, "InjectMarkerMembers") == true);

        var searchRangeTemplate = ResolveSearchRangeTemplate(
            crudAttr?.NamedArguments,
            stringKey: "SearchRangeTemplate",
            typeKey: "SearchRangeType",
            fallback: options.SearchRangeTemplate);

        foreach (var mode in classModes)
        {
            if (!string.IsNullOrWhiteSpace(mode.Inherit) && !CrudPresetExpander.IsValidTypeName(mode.Inherit))
            {
                diagnostics.Add(GeneratorDiagnostics.Create(
                    GeneratorDiagnostics.InvalidInheritStr,
                    location,
                    symbol.ToDisplayString(),
                    mode.Id,
                    mode.Inherit!));
            }

            foreach (var attr in mode.Attributes)
            {
                if (!SyntaxGenerationHelpers.TryParseAttributeList(attr, out _))
                {
                    diagnostics.Add(GeneratorDiagnostics.Create(
                        GeneratorDiagnostics.InvalidAttributeSyntax,
                        location,
                        symbol.ToDisplayString(),
                        attr));
                }
            }
        }

        var properties = CollectProperties(
            symbol,
            classModes,
            location,
            diagnostics,
            injectMarkerMembers,
            searchRangeTemplate!,
            cancellationToken);
        var customPropertyAttributes = CollectCustomPropertyAttributes(symbol);
        var modeIds = new HashSet<string>(classModes.Select(m => m.Id), StringComparer.Ordinal);

        // modeId -> generated type name (for Inherit dedupe)
        var modeTypeNames = classModes.ToDictionary(
            m => m.Id,
            m => m.Prefix + m.Name + m.Suffix,
            StringComparer.Ordinal);
        var inheritBaseModeId = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var mode in classModes)
        {
            if (string.IsNullOrWhiteSpace(mode.Inherit))
                continue;
            var baseMode = classModes.FirstOrDefault(m =>
                string.Equals(modeTypeNames[m.Id], mode.Inherit, StringComparison.Ordinal));
            if (baseMode is not null)
                inheritBaseModeId[mode.Id] = baseMode.Id;
        }

        if (properties.Length > 0 && classModes.Length == 0)
        {
            diagnostics.Add(GeneratorDiagnostics.Create(
                GeneratorDiagnostics.PropertyWithoutClassModes,
                location,
                symbol.ToDisplayString()));
        }

        var reportedUnbound = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in properties)
        {
            foreach (var modeId in property.AllModeIds())
            {
                if (modeIds.Contains(modeId))
                    continue;

                var key = property.SourceName + "\0" + modeId;
                if (!reportedUnbound.Add(key))
                    continue;

                diagnostics.Add(GeneratorDiagnostics.Create(
                    GeneratorDiagnostics.UnboundPropertyMode,
                    location,
                    symbol.ToDisplayString(),
                    property.SourceName,
                    modeId));
            }
        }

        var classInfos = ImmutableArray.CreateBuilder<SourceGeneratorClassInfo>();
        foreach (var mode in classModes)
        {
            var classProperties = ImmutableArray.CreateBuilder<SourceGeneratorPropertyInfo>();
            var seenPropertyNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var property in properties.Where(p => p.MatchesMode(mode.Id)))
            {
                var propertyName = property.Prefix + property.Name + property.Suffix;

                // Detail/Create/Update 继承 Dto 且同名同类型时跳过，避免 CS0108。
                if (inheritBaseModeId.TryGetValue(mode.Id, out var baseModeId)
                    && IsCoveredByInheritedMode(properties, baseModeId, propertyName, property))
                {
                    continue;
                }

                if (!seenPropertyNames.Add(propertyName))
                {
                    diagnostics.Add(GeneratorDiagnostics.Create(
                        GeneratorDiagnostics.DuplicatePropertyName,
                        location,
                        mode.Prefix + mode.Name + mode.Suffix,
                        propertyName));
                    continue;
                }

                if (CountTrue(property.IsVirtual, property.IsOverride, property.IsNew) > 1)
                {
                    diagnostics.Add(GeneratorDiagnostics.Create(
                        GeneratorDiagnostics.InvalidPropertyModifier,
                        location,
                        mode.Prefix + mode.Name + mode.Suffix,
                        propertyName));
                    continue;
                }

                var isInit = property.IsInit == true && mode.ClassStyle != ClassStyleEnum.Inpc;
                var isRequired = property.IsRequired == true && mode.ClassStyle != ClassStyleEnum.Inpc;

                classProperties.Add(new SourceGeneratorPropertyInfo(
                    Name: propertyName,
                    Modifier: property.Modifier,
                    Summary: property.SummaryPrefix + property.Summary + property.SummarySuffix,
                    Remarks: property.Remarks,
                    Example: property.Example,
                    IsVirtual: property.IsVirtual,
                    IsOverride: property.IsOverride,
                    IsNew: property.IsNew,
                    Type: property.Type,
                    DefaultValue: property.DefaultValue,
                    IsNullable: property.IsNullable,
                    Attributes: ToImmutableStringArray(
                        property.Attributes.Concat(customPropertyAttributes
                            .Where(ff => ff.SourceName == property.Name
                                && ff.MatchesMode(mode.Id))
                            .SelectMany(ff => ff.Attributes))),
                    InheritAttributes: property.InheritAttributes,
                    IsInit: isInit,
                    IsRequired: isRequired));

                foreach (var attr in property.Attributes)
                {
                    if (!SyntaxGenerationHelpers.TryParseAttributeList(attr, out _))
                    {
                        diagnostics.Add(GeneratorDiagnostics.Create(
                            GeneratorDiagnostics.InvalidAttributeSyntax,
                            location,
                            symbol.ToDisplayString(),
                            attr));
                    }
                }
            }

            classInfos.Add(new SourceGeneratorClassInfo(
                Name: mode.Prefix + mode.Name + mode.Suffix,
                Modifier: mode.Modifier,
                Summary: mode.SummaryPrefix + mode.Summary + mode.SummarySuffix,
                Remarks: mode.Remarks,
                Example: mode.Example,
                ClassNamespace: mode.NamespacePrefix + mode.Namespace + mode.NamespaceSuffix,
                Usings: mode.Usings,
                Interfaces: mode.Interfaces,
                IsAbstract: mode.IsAbstract,
                Inherit: mode.Inherit,
                Properties: classProperties.ToImmutable(),
                Attributes: mode.Attributes,
                IsPartial: mode.IsPartial,
                InheritAttribute: mode.InheritAttribute,
                ToNullable: mode.ToNullable,
                ClassStyle: mode.ClassStyle));
        }

        var builtClassInfos = classInfos.ToImmutable();
        var modesImmutable = classModes.ToImmutableArray();

        InterfaceContractChecker.ReportMissingMembers(
            symbol,
            modesImmutable,
            builtClassInfos,
            location,
            diagnostics);

        if (!indexColumnRefs.IsDefaultOrEmpty)
        {
            var entityInfo = builtClassInfos.FirstOrDefault(c =>
            {
                var entityMode = classModes.FirstOrDefault(m => m.Id == AutoCodeModeIds.Entity);
                return entityMode is not null
                    && string.Equals(c.Name, entityMode.Prefix + entityMode.Name + entityMode.Suffix, StringComparison.Ordinal);
            });
            var entityPropNames = new HashSet<string>(
                entityInfo?.Properties.Select(p => p.Name!).Where(n => !string.IsNullOrEmpty(n))
                ?? [],
                StringComparer.Ordinal);

            foreach (var (indexName, column) in indexColumnRefs)
            {
                if (entityPropNames.Contains(column))
                    continue;

                diagnostics.Add(GeneratorDiagnostics.Create(
                    GeneratorDiagnostics.IndexColumnNotGenerated,
                    location,
                    symbol.ToDisplayString(),
                    indexName,
                    column));
            }
        }

        return (autoCodeNullable, builtClassInfos, diagnostics.ToImmutable());
    }

    private static bool IsCoveredByInheritedMode(
        ImmutableArray<PropertyBindingInfo> properties,
        string baseModeId,
        string propertyName,
        PropertyBindingInfo property)
    {
        foreach (var baseProp in properties.Where(p => p.MatchesMode(baseModeId)))
        {
            var baseName = baseProp.Prefix + baseProp.Name + baseProp.Suffix;
            if (!string.Equals(baseName, propertyName, StringComparison.Ordinal))
                continue;
            if (!string.Equals(NormalizeType(baseProp.Type), NormalizeType(property.Type), StringComparison.Ordinal))
                continue;
            return true;
        }

        return false;
    }

    private static string? NormalizeType(string? type)
        => type?.Replace(" ", string.Empty);

    private static ImmutableArray<PropertyBindingInfo> CollectProperties(
        INamedTypeSymbol symbol,
        ClassModeInfo[] classModes,
        Location location,
        ImmutableArray<Diagnostic>.Builder diagnostics,
        bool injectMarkerMembers,
        string searchRangeTemplate,
        CancellationToken cancellationToken)
    {
        var seenSymbols = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);
        var result = ImmutableArray.CreateBuilder<PropertyBindingInfo>();

        foreach (var property in EnumerateDistinctProperties(symbol))
        {
            if (!seenSymbols.Add(property))
                continue;

            foreach (var attribute in property.GetAttributes().Where(
                f => f.AttributeClass?.ToDisplayString() == AutoCodePropertyAttribute))
            {
                var id = GetValue<string>(attribute.NamedArguments, "Id");
                var ids = ToImmutableStringArray(GetValues<string>(attribute.NamedArguments, "Ids"));
                var modeFlags = GetValue<int?>(attribute.NamedArguments, "ModeFlags") ?? 0;
                if (modeFlags != 0)
                    ids = ids.AddRange(ExpandModeFlags(modeFlags));

                result.Add(new PropertyBindingInfo(
                    SourceName: property.Name,
                    Name: GetValue<string>(attribute.NamedArguments, "Name") ?? property.Name,
                    Prefix: GetValue<string>(attribute.NamedArguments, "Prefix"),
                    Suffix: GetValue<string>(attribute.NamedArguments, "Suffix"),
                    Modifier: GetValue<Accessibility?>(attribute.NamedArguments, "Modifier") ?? property.DeclaredAccessibility,
                    Attributes: ToImmutableStringArray(GetValues<string>(attribute.NamedArguments, "Attributes")),
                    Remarks: GetValue<string>(attribute.NamedArguments, "Remarks"),
                    Example: GetValue<string>(attribute.NamedArguments, "Example"),
                    SummaryPrefix: GetValue<string>(attribute.NamedArguments, "SummaryPrefix"),
                    SummarySuffix: GetValue<string>(attribute.NamedArguments, "SummarySuffix"),
                    Summary: GetValue<string>(attribute.NamedArguments, "Summary")
                        ?? GetDocSummary(property.GetDocumentationCommentXml(cancellationToken: cancellationToken))
                        ?? property.Name,
                    Id: id,
                    Ids: ids,
                    DefaultValue: GetValue<string>(attribute.NamedArguments, "DefaultValue"),
                    Type: GetValue<string>(attribute.NamedArguments, "PropertyType")
                        ?? GetValue<string>(attribute.NamedArguments, "PropertyTypeStr")
                        ?? property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)?.TrimEnd('?'),
                    IsVirtual: GetValue<bool?>(attribute.NamedArguments, "IsVirtual"),
                    IsOverride: GetValue<bool?>(attribute.NamedArguments, "IsOverride"),
                    IsNew: GetValue<bool?>(attribute.NamedArguments, "IsNew"),
                    IsNullable: GetValue<bool?>(attribute.NamedArguments, "IsNullable")
                        ?? property.NullableAnnotation == NullableAnnotation.Annotated,
                    InheritAttributes: ImmutableArray.CreateRange(
                        property.GetAttributes()
                            .Where(ff => ff.AttributeClass?.ToDisplayString() is { } name
                                && name != AutoCodePropertyAttribute
                                && name != CustomAttributeAttribute
                                && name != CrudPresetExpander.AutoCodeRelationAttribute
                                && name != MorphExpander.AutoCodeMorphAttribute
                                && name != ColumnIndexExpander.AutoCodeColumnAttribute)
                            .Select(GetAttributeContent)
                            .Where(content => content is not null)
                            .Cast<string>()),
                    IsInit: GetValue<bool?>(attribute.NamedArguments, "IsInit"),
                    IsRequired: GetValue<bool?>(attribute.NamedArguments, "IsRequired")));
            }
        }

        CrudPresetExpander.ExpandRelations(
            symbol,
            classModes.ToImmutableArray(),
            result,
            location,
            diagnostics,
            cancellationToken);

        MorphExpander.ExpandMorphs(symbol, result, searchRangeTemplate, cancellationToken);
        ColumnIndexExpander.ExpandColumns(symbol, result);

        if (injectMarkerMembers)
            MarkerMemberInjector.Inject(classModes, result, searchRangeTemplate);

        return result.ToImmutable();
    }

    private static IEnumerable<IPropertySymbol> EnumerateDistinctProperties(INamedTypeSymbol symbol)
    {
        var seenNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var member in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (seenNames.Add(member.Name))
                yield return member;
        }

        foreach (var iface in symbol.AllInterfaces.OrderByDescending(i => i.AllInterfaces.Length))
        {
            foreach (var member in iface.GetMembers().OfType<IPropertySymbol>())
            {
                if (seenNames.Add(member.Name))
                    yield return member;
            }
        }
    }

    private static ImmutableArray<CustomPropertyAttributeInfo> CollectCustomPropertyAttributes(INamedTypeSymbol symbol)
    {
        var result = ImmutableArray.CreateBuilder<CustomPropertyAttributeInfo>();
        var seenSymbols = new HashSet<IPropertySymbol>(SymbolEqualityComparer.Default);

        foreach (var property in EnumerateDistinctProperties(symbol))
        {
            if (!seenSymbols.Add(property))
                continue;

            foreach (var attribute in property.GetAttributes().Where(
                f => f.AttributeClass?.ToDisplayString() == CustomAttributeAttribute))
            {
                result.Add(new CustomPropertyAttributeInfo(
                    SourceName: property.Name,
                    Id: GetValue<string>(attribute.NamedArguments, "Id"),
                    Ids: ToImmutableStringArray(GetValues<string>(attribute.NamedArguments, "Ids")),
                    Attributes: ToImmutableStringArray(GetValues<string>(attribute.NamedArguments, "Attributes"))));
            }
        }

        return result.ToImmutable();
    }

    private static int CountTrue(params bool?[] values)
        => values.Count(v => v == true);

    private static string GetCSharpString(TypedConstant typedConstant)
        => typedConstant.Kind == TypedConstantKind.Array
            ? "new[]" + typedConstant.ToCSharpString()
            : typedConstant.ToCSharpString();

    private static string? GetAttributeContent(AttributeData attribute)
    {
        if (attribute.AttributeClass is null)
            return null;

        var className = attribute.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (!attribute.ConstructorArguments.Any() && !attribute.NamedArguments.Any())
            return className;

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(className);
        stringBuilder.Append('(');

        var first = true;
        foreach (var constructorArgument in attribute.ConstructorArguments)
        {
            if (!first)
                stringBuilder.Append(", ");
            stringBuilder.Append(GetCSharpString(constructorArgument));
            first = false;
        }

        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (!first)
                stringBuilder.Append(", ");
            stringBuilder.Append(namedArgument.Key);
            stringBuilder.Append(" = ");
            stringBuilder.Append(GetCSharpString(namedArgument.Value));
            first = false;
        }

        stringBuilder.Append(')');
        return stringBuilder.ToString();
    }

    private static string? GetDocSummary(string? comment)
    {
        if (string.IsNullOrEmpty(comment))
            return null;

        var xmlComment = XDocument.Parse(comment);
        return xmlComment.XPathSelectElement("member/summary")?.Value?.Trim();
    }

    private sealed record CustomPropertyAttributeInfo(
        string SourceName,
        string? Id,
        ImmutableArray<string> Ids,
        ImmutableArray<string> Attributes)
    {
        public bool MatchesMode(string modeId)
            => Id == modeId || Ids.Contains(modeId);
    }
}
