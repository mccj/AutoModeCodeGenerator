using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace 自动代码生成;

[Generator(LanguageNames.CSharp)]
public partial class AutoCodeGenerator : IIncrementalGenerator
{
    private const string autoCodeClassModesAttributeStr = "AutoCodeGenerator.AutoCodeClassModesAttribute";
    private const string autoCodePropertyAttributeStr = "AutoCodeGenerator.AutoCodePropertyAttribute";
    private const string autoCodeNullableAttributeStr = "AutoCodeGenerator.AutoCodeNullableAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //对于Source Generator可以通过添加`Debugger.Launch()`的形式进行对编译时的生成器进行调试，可以通过它很便捷的一步步调试代码.
        //Debugger.Launch();

        //AdditionalFiles 获取当前编译项目文件中的所有AdditionalFiles标签
        //Compilation 编译上下文，最重要的对象
        //AddSource 向编译器加入代码，最重要的方法广告

        context.RegisterPostInitializationOutput(ctx => { ctx.AddSource("AutoCodeAttribute.g.cs", SourceText.From(SourceGeneratorHelper.AutoCodeAttribute, Encoding.UTF8)); });

        var compilation = context.CompilationProvider;

        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(autoCodeClassModesAttributeStr,
            static (SyntaxNode syntaxNode, CancellationToken cancellationToken) =>
            {
                // 在此进行快速的语法判断逻辑，可以判断当前的内容是否感兴趣，如此过滤掉一些内容，从而减少后续处理，提升性能
                return syntaxNode.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ClassDeclaration) || syntaxNode.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InterfaceDeclaration);
            },
            static (GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) =>
            {
                var symbol = (INamedTypeSymbol)context.TargetSymbol;
                return symbol;
            });
        var combined = provider.Combine(compilation);

        context.RegisterSourceOutput(combined, static (ctx, result) =>
        {
            var symbol = result.Left;
            try
            {
                //Debugger.Launch();
                //var sss1 = symbol.ToDisplayString(new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
                //var sss2 = symbol.ToDisplayString(new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
                ////var sss2 = symbol.ToDisplayString(new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,                                                                                                   genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeRef | SymbolDisplayMemberOptions.IncludeContainingType, kindOptions: SymbolDisplayKindOptions.IncludeMemberKeyword, parameterOptions: SymbolDisplayParameterOptions.IncludeName | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeDefaultValue, localOptions: SymbolDisplayLocalOptions.IncludeType, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));
                //var sss3 = symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                var sources = GetClassInfo(symbol, ctx.CancellationToken);
                //
                var fileName = $"{(symbol.ContainingNamespace?.IsGlobalNamespace == true ? "_" : symbol.ContainingNamespace?.ToDisplayString())}.{symbol.Name}";
                var sourceCode = SourceGeneratorHelper.GeneratorCode(sources.nullable, sources.classInfos);

                ctx.AddSource($"{fileName}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                var InvalidError = new DiagnosticDescriptor(id: "Mccj001", title: "代码生成异常", messageFormat: "代码生成异常 '{0}'-{1}.", category: nameof(AutoCodeGenerator), DiagnosticSeverity.Error, isEnabledByDefault: true, description: ex.StackTrace);
                ctx.ReportDiagnostic(Diagnostic.Create(InvalidError, Location.None, $"{symbol.ToDisplayString()}.g.cs", ex.Message));
            }
        });

    }

    private static T? getValue<T>(TypedConstant? typedConstant)
    {
        if (!typedConstant.HasValue || typedConstant.Value.IsNull)
            return default;
        var type = typeof(T);
        if (typedConstant.Value.Kind == TypedConstantKind.Type && type == typeof(string) && typedConstant.Value.Value is ITypeSymbol typeSymbol)
            return (T?)(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) as object);

        if (typedConstant.Value.Kind == TypedConstantKind.Enum)
        {
            var t = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? type.GenericTypeArguments.FirstOrDefault() : type;
            var v = Enum.ToObject(t, typedConstant.Value.Value);
            return (T)v;
        }
        //    return (T?)(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) as object);

        if (typedConstant.Value.Kind != TypedConstantKind.Primitive) throw new Exception("不是数组类型");
        return (T?)typedConstant.Value.Value;
    }

    private static T? getValue<T>(ImmutableArray<KeyValuePair<string, TypedConstant>>? namedArguments, string key)
    {
        var typedConstant = namedArguments?.FirstOrDefault(f => f.Key == key).Value;
        return getValue<T>(typedConstant);
    }
    private static T?[]? getValues<T>(ImmutableArray<KeyValuePair<string, TypedConstant>>? namedArguments, string key, bool nul = true)
    {
        var typedConstant = namedArguments?.FirstOrDefault(f => f.Key == key).Value;
        if (!typedConstant.HasValue || typedConstant.Value.IsNull)
            return default;
        if (typedConstant.Value.Kind != TypedConstantKind.Array) throw new Exception("不是数组类型");
        var value = typedConstant.Value.Values.Select(f => getValue<T>(f)).Where(f => nul ? f != null : true).ToArray();
        return value;
    }
    private static string getCSharpString(TypedConstant typedConstant)
    {
        if (typedConstant.Kind == TypedConstantKind.Array)
            return "new[]" + typedConstant.ToCSharpString();
        return typedConstant.ToCSharpString();
    }
    private static string? getDocSummary(string? comment)
    {
        if (string.IsNullOrEmpty(comment))
            return null;
        var xmlComment = XDocument.Parse(comment);

        var value = xmlComment.XPathSelectElement("member/summary")?.Value?.Trim();

        return value;
    }
    private static (NullableEnum? nullable, SourceGeneratorClassInfo[] classInfos) GetClassInfo(INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        //System.Diagnostics.Debugger.Launch();

        //var symbolDisplayFormat = new SymbolDisplayFormat
        //(
        //    // 带上命名空间和类型名
        //    globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
        //    // 命名空间之前加上 global 防止冲突
        //    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        //    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        //    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes
        //);
        var attributeDatas = symbol.GetAttributes();
        var autoCodeNullableAttribute = attributeDatas.FirstOrDefault(f => f.AttributeClass?.ToDisplayString() == autoCodeNullableAttributeStr);
        //var w4 = autoCodeNullableAttribute?.ConstructorArguments.FirstOrDefault();
        //var dsfsd = w4.HasValue ? getValue<NullableEnum?>(w4.Value) : null;
        var autoCodeNullable = getValue<NullableEnum?>(autoCodeNullableAttribute?.NamedArguments, "Type") ?? NullableEnum.Restore;
        var getAttributeContent = (AttributeData attribute) =>
        {
            //Microsoft.CodeAnalysis.CSharp.Symbols.SourceAttributeData
            //Microsoft.CodeAnalysis.CSharp.Symbols.CSharpAttributeData
            if (attribute == null || attribute.AttributeClass == null) return null;

            string className = attribute.AttributeClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (!attribute.ConstructorArguments.Any() & !attribute.NamedArguments.Any())
            {
                return className;
            }

            var stringBuilder = new StringBuilder();

            stringBuilder.Append(className);
            stringBuilder.Append("(");

            var ss1 = attribute.ConstructorArguments.Select(constructorArgument => getCSharpString(constructorArgument)).ToArray();
            var ss2 = attribute.NamedArguments.Select(namedArgument => namedArgument.Key + " = " + getCSharpString(namedArgument.Value)).ToArray();
            bool first = true;

            foreach (var constructorArgument in attribute.ConstructorArguments)
            {
                if (!first)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(getCSharpString(constructorArgument));
                first = false;
            }

            foreach (var namedArgument in attribute.NamedArguments)
            {
                if (!first)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(namedArgument.Key);
                stringBuilder.Append(" = ");
                stringBuilder.Append(getCSharpString(namedArgument.Value));
                first = false;
            }

            stringBuilder.Append(")");

            return stringBuilder.ToString();
        };
        var ss1 = attributeDatas.Where(f => f.AttributeClass?.ToDisplayString() == autoCodeClassModesAttributeStr).Select(f => new
        {
            Name = getValue<string>(f.NamedArguments, "Name") ?? symbol.Name,
            Prefix = getValue<string>(f.NamedArguments, "Prefix"),
            Suffix = getValue<string>(f.NamedArguments, "Suffix"),
            Modifier = getValue<Accessibility?>(f.NamedArguments, "Modifier") ?? symbol.DeclaredAccessibility,
            IsPartial = getValue<bool?>(f.NamedArguments, "IsPartial") ?? true,
            Attributes = getValues<string>(f.NamedArguments, "Attributes") as string[],
            Remarks = getValue<string>(f.NamedArguments, "Remarks"),
            Example = getValue<string>(f.NamedArguments, "Example"),
            SummaryPrefix = getValue<string>(f.NamedArguments, "SummaryPrefix"),
            SummarySuffix = getValue<string>(f.NamedArguments, "SummarySuffix"),
            Summary = getValue<string>(f.NamedArguments, "Summary") ?? getDocSummary(symbol.GetDocumentationCommentXml(cancellationToken: cancellationToken)) ?? symbol.Name,
            Id = getValue<string>(f.NamedArguments, "Id"),
            Inherit = getValue<string>(f.NamedArguments, "InheritType") ?? getValue<string>(f.NamedArguments, "InheritStr"),
            Interfaces = (getValues<string>(f.NamedArguments, "InterfaceTypes") ?? getValues<string>(f.NamedArguments, "InterfaceStrs")) as string[],
            Usings = getValues<string>(f.NamedArguments, "Usings") as string[],
            Namespace = getValue<string>(f.NamedArguments, "Namespace") ?? (symbol.ContainingNamespace?.IsGlobalNamespace == true ? "AutoGenerator" : symbol.ContainingNamespace?.ToDisplayString() + ".AutoGenerator"),
            NamespacePrefix = getValue<string>(f.NamedArguments, "NamespacePrefix"),
            NamespaceSuffix = getValue<string>(f.NamedArguments, "NamespaceSuffix"),
            IsAbstract = getValue<bool?>(f.NamedArguments, "IsAbstract"),
            InheritAttribute = getValue<bool?>(f.NamedArguments, "InheritAttribute") ?? false,
            ToNullable = getValue<bool?>(f.NamedArguments, "ToNullable") ?? false,
        }).ToArray();

        var ss2 = symbol.GetMembers()
                 .Select(member => member as IPropertySymbol)
                 .Where(property => property != null)
                 .SelectMany(property => property?.GetAttributes().Where(f => f.AttributeClass?.ToDisplayString() == autoCodePropertyAttributeStr).Select(f => new
                 {
                     Name = getValue<string>(f.NamedArguments, "Name") ?? property.Name,
                     Prefix = getValue<string>(f.NamedArguments, "Prefix"),
                     Suffix = getValue<string>(f.NamedArguments, "Suffix"),
                     Modifier = getValue<Accessibility?>(f.NamedArguments, "Modifier") ?? property.DeclaredAccessibility,
                     Attributes = getValues<string>(f.NamedArguments, "Attributes") as string[],
                     Remarks = getValue<string>(f.NamedArguments, "Remarks"),
                     Example = getValue<string>(f.NamedArguments, "Example"),
                     SummaryPrefix = getValue<string>(f.NamedArguments, "SummaryPrefix"),
                     SummarySuffix = getValue<string>(f.NamedArguments, "SummarySuffix"),
                     Summary = getValue<string>(f.NamedArguments, "Summary") ?? getDocSummary(property.GetDocumentationCommentXml(cancellationToken: cancellationToken)) ?? property.Name,
                     Id = getValue<string>(f.NamedArguments, "Id"),
                     Ids = getValues<string>(f.NamedArguments, "Ids") as string[],
                     DefaultValue = getValue<string>(f.NamedArguments, "DefaultValue"),
                     Type = getValue<string>(f.NamedArguments, "PropertyType") ?? getValue<string>(f.NamedArguments, "PropertyTypeStr") ?? property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)?.TrimEnd('?'),
                     IsVirtual = getValue<bool?>(f.NamedArguments, "IsVirtual"),
                     IsNullable = getValue<bool?>(f.NamedArguments, "IsNullable") ?? property.NullableAnnotation == NullableAnnotation.Annotated,
                     InheritAttributes = property.GetAttributes().Where(ff => ff.AttributeClass?.ToDisplayString() != autoCodePropertyAttributeStr).Select(ff => getAttributeContent(ff)).ToArray()
                 })).ToArray();

        var sourceGeneratorClassInfos = ss1.Select(f => new SourceGeneratorClassInfo
        {
            Name = f.Name,
            Prefix = f.Prefix,
            Suffix = f.Suffix,
            Modifier = f.Modifier,
            IsPartial = f.IsPartial,
            Attributes = f.Attributes,
            Remarks = f.Remarks,
            Example = f.Example,
            Summary = f.Summary,
            SummaryPrefix = f.SummaryPrefix,
            SummarySuffix = f.SummarySuffix,
            ClassNamespace = f.Namespace,
            ClassNamespaceSuffix = f.NamespaceSuffix,
            ClassNamespacePrefix = f.NamespacePrefix,
            Usings = f.Usings,
            Interfaces = f.Interfaces,
            IsAbstract = f.IsAbstract,
            Inherit = f.Inherit,
            InheritAttribute = f.InheritAttribute,
            ToNullable = f.ToNullable,
            Propertes = ss2.Where(property => property.Id == f.Id || (property.Ids ?? new string[] { }).Contains(f.Id)).Select(property => new SourceGeneratorPropertyInfo
            {
                Name = property.Name,
                Prefix = property.Prefix,
                Suffix = property.Suffix,
                Modifier = property.Modifier,
                Attributes = property.Attributes,
                Remarks = property.Remarks,
                Example = property.Example,
                Summary = property.Summary,
                SummaryPrefix = property.SummaryPrefix,
                SummarySuffix = property.SummarySuffix,
                IsVirtual = property.IsVirtual,
                Type = property.Type,
                IsNullable = property.IsNullable,
                DefaultValue = property.DefaultValue,
                InheritAttributes = property.InheritAttributes,
            }).ToArray()
        })
        .ToArray();

        return (nullable: autoCodeNullable, classInfos: sourceGeneratorClassInfos);
    }
}
