using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace 自动代码生成;

/// <summary>
/// 从语义模型提取生成配置：解析特性、收集属性、组装 <see cref="SourceGeneratorClassInfo"/>。
/// </summary>
internal static class GenerationModelExtractor
{
    internal const string AutoCodeClassModesAttribute = "AutoCodeGenerator.AutoCodeClassModesAttribute";
    internal const string AutoCodePropertyAttribute = "AutoCodeGenerator.AutoCodePropertyAttribute";
    internal const string AutoCodeNullableAttribute = "AutoCodeGenerator.AutoCodeNullableAttribute";
    internal const string CustomAttributeAttribute = "AutoCodeGenerator.CustomAttributeAttribute";

    public static GenerationResult Extract(INamedTypeSymbol symbol, Location location, CancellationToken cancellationToken)
    {
        var fileName = GetFileName(symbol);
        try
        {
            var (nullable, classInfos, diagnostics) = BuildModel(symbol, location, cancellationToken);
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

        var classModes = attributeDatas
            .Where(f => f.AttributeClass?.ToDisplayString() == AutoCodeClassModesAttribute)
            .Select(mode => new { mode.NamedArguments, Id = GetValue<string>(mode.NamedArguments, "Id") })
            .Select(mode => new ClassModeInfo(
                Id: mode.Id,
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
                Summary: GetValue<string>(mode.NamedArguments, "Summary")
                    ?? GetDocSummary(symbol.GetDocumentationCommentXml(cancellationToken: cancellationToken))
                    ?? symbol.Name,
                Inherit: GetValue<string>(mode.NamedArguments, "InheritType")
                    ?? GetValue<string>(mode.NamedArguments, "InheritStr"),
                Interfaces: ToImmutableStringArray(
                    GetValues<string>(mode.NamedArguments, "InterfaceTypes")
                    ?? GetValues<string>(mode.NamedArguments, "InterfaceStrs")),
                Usings: ToImmutableStringArray(GetValues<string>(mode.NamedArguments, "Usings")),
                Namespace: GetValue<string>(mode.NamedArguments, "Namespace")
                    ?? (symbol.ContainingNamespace?.IsGlobalNamespace == true
                        ? "AutoGenerator"
                        : symbol.ContainingNamespace?.ToDisplayString() + ".AutoGenerator"),
                NamespacePrefix: GetValue<string>(mode.NamedArguments, "NamespacePrefix"),
                NamespaceSuffix: GetValue<string>(mode.NamedArguments, "NamespaceSuffix"),
                IsAbstract: GetValue<bool?>(mode.NamedArguments, "IsAbstract"),
                InheritAttribute: GetValue<bool?>(mode.NamedArguments, "InheritAttribute") ?? false,
                ToNullable: GetValue<bool?>(mode.NamedArguments, "ToNullable") ?? false,
                ClassStyle: GetValue<ClassStyleEnum?>(mode.NamedArguments, "ClassStyle") ?? ClassStyleEnum.Poco))
            .Where(mode => !string.IsNullOrWhiteSpace(mode.Id))
            .ToArray();

        var properties = CollectProperties(symbol, cancellationToken);
        var customPropertyAttributes = CollectCustomPropertyAttributes(symbol);

        var classInfos = ImmutableArray.CreateBuilder<SourceGeneratorClassInfo>();
        foreach (var mode in classModes)
        {
            var classProperties = ImmutableArray.CreateBuilder<SourceGeneratorPropertyInfo>();
            var seenPropertyNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var property in properties.Where(p => p.MatchesMode(mode.Id!)))
            {
                var propertyName = property.Prefix + property.Name + property.Suffix;
                if (!seenPropertyNames.Add(propertyName))
                    continue;

                if (CountTrue(property.IsVirtual, property.IsOverride, property.IsNew) > 1)
                {
                    diagnostics.Add(GeneratorDiagnostics.Create(
                        GeneratorDiagnostics.InvalidPropertyModifier,
                        location,
                        mode.Prefix + mode.Name + mode.Suffix,
                        propertyName));
                    continue;
                }

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
                                && ff.MatchesMode(mode.Id!))
                            .SelectMany(ff => ff.Attributes))),
                    InheritAttributes: property.InheritAttributes));
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

        return (autoCodeNullable, classInfos.ToImmutable(), diagnostics.ToImmutable());
    }

    private static ImmutableArray<PropertyBindingInfo> CollectProperties(INamedTypeSymbol symbol, CancellationToken cancellationToken)
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
                    Id: GetValue<string>(attribute.NamedArguments, "Id"),
                    Ids: ToImmutableStringArray(GetValues<string>(attribute.NamedArguments, "Ids")),
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
                                && name != CustomAttributeAttribute)
                            .Select(GetAttributeContent)
                            .Where(content => content is not null)
                            .Cast<string>())));
            }
        }

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

    private static ImmutableArray<string> ToImmutableStringArray(IEnumerable<string?>? values)
        => ImmutableArray.CreateRange(
            (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!));

    private static T? GetValue<T>(TypedConstant typedConstant)
        => GetValue<T>((TypedConstant?)typedConstant);

    private static T? GetValue<T>(TypedConstant? typedConstant)
    {
        if (!typedConstant.HasValue || typedConstant.Value.IsNull)
            return default;

        var type = typeof(T);
        if (typedConstant.Value.Kind == TypedConstantKind.Type
            && type == typeof(string)
            && typedConstant.Value.Value is ITypeSymbol typeSymbol)
        {
            return (T?)(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) as object);
        }

        if (typedConstant.Value.Kind == TypedConstantKind.Enum)
        {
            var enumType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? type.GenericTypeArguments.FirstOrDefault()
                : type;
            var value = Enum.ToObject(enumType!, typedConstant.Value.Value!);
            return (T)value;
        }

        if (typedConstant.Value.Kind != TypedConstantKind.Primitive)
            throw new InvalidOperationException($"不支持的特性参数类型: {typedConstant.Value.Kind}");

        return (T?)typedConstant.Value.Value;
    }

    private static T? GetValue<T>(ImmutableArray<KeyValuePair<string, TypedConstant>>? namedArguments, string key)
    {
        var typedConstant = namedArguments?.FirstOrDefault(f => f.Key == key).Value;
        return GetValue<T>(typedConstant);
    }

    private static T?[]? GetValues<T>(ImmutableArray<KeyValuePair<string, TypedConstant>>? namedArguments, string key, bool filterNull = true)
    {
        var typedConstant = namedArguments?.FirstOrDefault(f => f.Key == key).Value;
        if (!typedConstant.HasValue || typedConstant.Value.IsNull)
            return default;

        if (typedConstant.Value.Kind != TypedConstantKind.Array)
            throw new InvalidOperationException("特性参数不是数组类型");

        return typedConstant.Value.Values
            .Select(static v => GetValue<T>(v))
            .Where(v => !filterNull || v is not null)
            .ToArray();
    }

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

    private sealed record ClassModeInfo(
        string? Id,
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
        ImmutableArray<string> Usings,
        string? Namespace,
        string? NamespacePrefix,
        string? NamespaceSuffix,
        bool? IsAbstract,
        bool InheritAttribute,
        bool ToNullable,
        ClassStyleEnum ClassStyle);

    private sealed record PropertyBindingInfo(
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
        ImmutableArray<string> InheritAttributes)
    {
        public bool MatchesMode(string modeId)
            => Id == modeId || Ids.Contains(modeId);
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
