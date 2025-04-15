﻿using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using 自动代码生成;

//namespace 自动代码生成;

[Generator(LanguageNames.CSharp)]
public partial class AutoCodeGenerator : IIncrementalGenerator
{
    private const string autoCodeClassModesAttributeStr = "AutoCodeGenerator.AutoCodeClassModesAttribute";
    private const string autoCodePropertyAttributeStr = "AutoCodeGenerator.AutoCodePropertyAttribute";
    private const string autoCodeNullableAttributeStr = "AutoCodeGenerator.AutoCodeNullableAttribute";
    private const string customAttributeAttributeStr = "AutoCodeGenerator.CustomAttributeAttribute";
    private string autoCodeAttribute => SourceGeneratorHelper.AutoGenerated + @"
#nullable enable
using System;
namespace AutoCodeGenerator
{
    /// <summary>
    /// 
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public abstract class AutoCodeClassBaseAttribute : Attribute
    {
        /// <summary>
        /// 关联id
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 前缀
        /// </summary>
        public string? Prefix { get; set; }
        /// <summary>
        /// 后缀
        /// </summary>
        public string? Suffix { get; set; }
        /// <summary>
        /// 修饰符
        /// </summary>
        public AccessibilityEnum Modifier { get; set; }
        /// <summary>
        /// 特性
        /// </summary>
        public string[]? Attributes { get; set; }
        /// <summary>
        /// 注释 描述
        /// </summary>
        public string? Summary { get; set; }
        /// <summary>
        /// 注释 前缀
        /// </summary>
        public string? SummaryPrefix { get; set; }
        /// <summary>
        /// 注释 后缀
        /// </summary>
        public string? SummarySuffix { get; set; }
        /// <summary>
        /// 注释 示例
        /// </summary>
        public string? Example { get; set; }
        /// <summary>
        /// 注释 评论
        /// </summary>
        /// <remarks></remarks>
        public string? Remarks { get; set; }
        /// <summary>
        /// 分部类型
        /// </summary>
        public bool IsPartial { get; set; }
    }

    /// <summary>
    /// Nullable
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AutoCodeNullableAttribute : Attribute
    {
        // public AutoCodeNullableAttribute(NullableEnum t)
        // {
        //     this.Type = t;
        // }
        /// <summary>
        ///  Nullable 类型
        /// </summary>
        public NullableEnum Type { get; set; }
    }

    /// <summary>
    /// 接口
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class AutoCodeClassModesAttribute : AutoCodeClassBaseAttribute
    {
        /// <summary>
        /// 继承
        /// </summary>
        public string? InheritStr { get; set; }

        /// <summary>
        /// 继承
        /// </summary>
        public Type? InheritType { get; set; }
        /// <summary>
        /// 接口
        /// </summary>
        public string[]? InterfaceStrs { get; set; }
        /// <summary>
        /// 接口
        /// </summary>
        public Type[]? InterfaceTypes { get; set; }
        /// <summary>
        /// 命名空间
        /// </summary>
        public string? Namespace { get; set; } = string.Empty;
        /// <summary>
        /// 命名空间前缀
        /// </summary>
        public string? NamespacePrefix { get; set; }
        /// <summary>
        /// 命名空间后缀
        /// </summary>
        public string? NamespaceSuffix { get; set; }
        /// <summary>
        /// 派生类
        /// </summary>
        public bool IsAbstract { get; set; }
        /// <summary>
        /// 命名空间引用
        /// </summary>
        public string[]? Usings { get; set; }
        /// <summary>
        /// 继承 Attribute
        /// </summary>
        public bool InheritAttribute { get; set; }
        /// <summary>
        /// 所有属性都允许空
        /// </summary>
        public bool ToNullable { get; set; }
        /// <summary>
        /// 生成的类库样式
        /// </summary>
        public ClassStyleEnum ClassStyle { get; set; }
    }

    /// <summary>
    /// 属性
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class AutoCodePropertyAttribute : AutoCodeClassBaseAttribute
    {
        /// <summary>
        /// 关联id
        /// </summary>
        public string[]? Ids { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string? PropertyTypeStr { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public Type? PropertyType { get; set; }
        /// <summary>
        /// 允许重写
        /// </summary>
        public bool IsVirtual { get; set; }
        /// <summary>
        /// 重写继承的成员
        /// </summary>
        public bool IsOverride { get; set; }
        /// <summary>
        /// 隐藏继承的成员
        /// </summary>
        public bool IsNew { get; set; }
        /// <summary>
        /// 允许空
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string? DefaultValue { get; set; }    }
    /// <summary>
    /// 修饰符
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum AccessibilityEnum
    {
        /// <summary>
        /// 没有 修饰符
        /// </summary>
        NotApplicable = 0,
        /// <summary>
        /// 访问限于包含类
        /// </summary>
        Private = 1,
        /// <summary>
        /// private protected 访问限于包含类或当前程序集中派生自包含类的类型
        /// </summary>
        ProtectedAndInternal = 2,
        /// <summary>
        /// 访问限于包含类或派生自包含类的类型
        /// </summary>
        Protected = 3,
        /// <summary>
        /// 访问限于当前程序集
        /// </summary>
        Internal = 4,
        /// <summary>
        /// protected internal 访问限于当前程序集或派生自包含类的类型
        /// </summary>
        ProtectedOrInternal = 5,
        /// <summary>
        /// 访问不受限制
        /// </summary>
        Public = 6
    }
    /// <summary>
    /// 
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum NullableEnum
    {
        /// <summary>
        /// 将可为空注释和警告上下文设置为“已禁用”。
        /// </summary>
        Disable,
        /// <summary>
        /// 将可为空注释和警告上下文设置为“已启用”。
        /// </summary>
        Enable,
        /// <summary>
        /// 将可为空注释和警告上下文还原为项目设置。
        /// </summary>
        Restore,
        /// <summary>
        /// 将可为空注释上下文设置为“已禁用”。
        /// </summary>
        DisableAnnotations,
        /// <summary>
        /// 将可为空注释上下文设置为“已启用”。
        /// </summary>
        EnableAnnotations,
        /// <summary>
        /// 将可为空注释上下文还原为项目设置。
        /// </summary>
        RestoreAnnotations,
        /// <summary>
        /// 将可为空警告上下文设置为“已禁用”。
        /// </summary>
        DisableWarnings,
        /// <summary>
        /// 将可为空警告上下文设置为“已启用”。
        /// </summary>
        EnableWarnings,
        /// <summary>
        /// 将可为空警告上下文还原为项目设置。
        /// </summary>
        RestoreWarnings
    }
    /// <summary>
    /// 生成的类库样式
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum ClassStyleEnum
    {
        Poco,
        Record,
        Inpc
    }
    /// <summary>
    /// Inpc 模式基类
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public abstract class InpcPropertyBindableBaseAbstract
    {
        protected internal global::System.Collections.Generic.Dictionary<string, object> keyValuePairs = new global::System.Collections.Generic.Dictionary<string, object>();
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <typeparam name=""T"">属性类型</typeparam>
        /// <param name=""defaultValue"">不存在当前值时，返回的默认值</param>
        /// <param name=""propertyName"">属性名称</param>
        /// <returns>返回的值，或者默认值</returns>
        protected virtual T GetValue<T>(T defaultValue = default, [global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            if (keyValuePairs.TryGetValue(propertyName, out var value))
                return (T)value;
            else return defaultValue;
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <typeparam name=""T"">属性类型</typeparam>
        /// <param name=""value"">需要设置的属性值</param>
        /// <param name=""propertyName"">属性名称</param>
        protected virtual void SetValue<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            keyValuePairs[propertyName] = value;
        }
        /// <summary>
        /// 属性修改时触发
        /// </summary>
        /// <param name=""propertyName""></param>
        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null) { }
    }
   /// <summary>
    /// 自定义 Attribute
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class CustomAttributeAttribute : Attribute
    {
        /// <summary>
        /// 关联id
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// 关联id
        /// </summary>
        public string[]? Ids { get; set; }
        /// <summary>
        ///  Nullable 类型
        /// </summary>
        public string[] Attributes { get; set; }
    }
}";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //对于Source Generator可以通过添加`Debugger.Launch()`的形式进行对编译时的生成器进行调试，可以通过它很便捷的一步步调试代码.
        //Debugger.Launch();

        //AdditionalFiles 获取当前编译项目文件中的所有AdditionalFiles标签
        //Compilation 编译上下文，最重要的对象
        //AddSource 向编译器加入代码，最重要的方法广告

        context.RegisterPostInitializationOutput(ctx => { ctx.AddSource("AutoCodeAttribute.g.cs", SourceText.From(autoCodeAttribute, Encoding.UTF8)); });

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
                //Debugger.Launch();
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
        var attributeDatas = symbol.GetAttributes().Concat(symbol.AllInterfaces.SelectMany(f => f.GetAttributes())).ToArray();
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

            //var ss111 = attribute.ConstructorArguments.Select(constructorArgument => getCSharpString(constructorArgument)).ToArray();
            //var ss222 = attribute.NamedArguments.Select(namedArgument => namedArgument.Key + " = " + getCSharpString(namedArgument.Value)).ToArray();
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
        var classModes = attributeDatas.Where(f => f.AttributeClass?.ToDisplayString() == autoCodeClassModesAttributeStr)
            .Select(f => new { f.NamedArguments, Id = getValue<string>(f.NamedArguments, "Id") })
            .Select(f => new
            {
                Name = getValue<string>(f.NamedArguments, "Name") ?? symbol.Name,
                Prefix = getValue<string>(f.NamedArguments, "Prefix"),
                Suffix = getValue<string>(f.NamedArguments, "Suffix"),
                Modifier = getValue<Accessibility?>(f.NamedArguments, "Modifier") ?? symbol.DeclaredAccessibility,
                IsPartial = getValue<bool?>(f.NamedArguments, "IsPartial") ?? true,
                Attributes = (getValues<string>(f.NamedArguments, "Attributes") as string[] ?? []).Concat(attributeDatas.Where(ff => ff.AttributeClass?.ToDisplayString() == customAttributeAttributeStr).Where(ff => getValue<string>(ff.NamedArguments, "Id") == f.Id || (getValues<string>(ff.NamedArguments, "Ids") ?? []).Contains(f.Id)).SelectMany(ff => getValues<string>(ff.NamedArguments, "Attributes") as string[] ?? []).ToArray()).ToArray(),
                Remarks = getValue<string>(f.NamedArguments, "Remarks"),
                Example = getValue<string>(f.NamedArguments, "Example"),
                SummaryPrefix = getValue<string>(f.NamedArguments, "SummaryPrefix"),
                SummarySuffix = getValue<string>(f.NamedArguments, "SummarySuffix"),
                Summary = getValue<string>(f.NamedArguments, "Summary") ?? getDocSummary(symbol.GetDocumentationCommentXml(cancellationToken: cancellationToken)) ?? symbol.Name,
                Id = f.Id,
                Inherit = getValue<string>(f.NamedArguments, "InheritType") ?? getValue<string>(f.NamedArguments, "InheritStr"),
                Interfaces = (getValues<string>(f.NamedArguments, "InterfaceTypes") ?? getValues<string>(f.NamedArguments, "InterfaceStrs")) as string[],
                Usings = getValues<string>(f.NamedArguments, "Usings") as string[],
                Namespace = getValue<string>(f.NamedArguments, "Namespace") ?? (symbol.ContainingNamespace?.IsGlobalNamespace == true ? "AutoGenerator" : symbol.ContainingNamespace?.ToDisplayString() + ".AutoGenerator"),
                NamespacePrefix = getValue<string>(f.NamedArguments, "NamespacePrefix"),
                NamespaceSuffix = getValue<string>(f.NamedArguments, "NamespaceSuffix"),
                IsAbstract = getValue<bool?>(f.NamedArguments, "IsAbstract"),
                InheritAttribute = getValue<bool?>(f.NamedArguments, "InheritAttribute") ?? false,
                ToNullable = getValue<bool?>(f.NamedArguments, "ToNullable") ?? false,
                ClassStyle = getValue<ClassStyleEnum?>(f.NamedArguments, "ClassStyle") ?? ClassStyleEnum.Poco,
            }).ToArray();

        var classMembers = symbol.GetMembers().Concat(symbol.AllInterfaces.SelectMany(f => f.GetMembers()))
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
                     IsOverride = getValue<bool?>(f.NamedArguments, "IsOverride"),
                     IsNew = getValue<bool?>(f.NamedArguments, "IsNew"),
                     IsNullable = getValue<bool?>(f.NamedArguments, "IsNullable") ?? property.NullableAnnotation == NullableAnnotation.Annotated,
                     InheritAttributes = property.GetAttributes().Where(ff => ff.AttributeClass?.ToDisplayString() != autoCodePropertyAttributeStr).Select(ff => getAttributeContent(ff)).ToArray()
                 })).ToArray();

        var sourceGeneratorClassInfos = classModes.Select(f => new SourceGeneratorClassInfo
        {
            Name = f.Prefix + f.Name + f.Suffix,
            //Prefix = f.Prefix,
            //Suffix = f.Suffix,
            Modifier = f.Modifier,
            IsPartial = f.IsPartial,
            Attributes = f.Attributes,
            Remarks = f.Remarks,
            Example = f.Example,
            Summary = f.SummaryPrefix + f.Summary + f.SummarySuffix,
            //SummaryPrefix = f.SummaryPrefix,
            //SummarySuffix = f.SummarySuffix,
            ClassNamespace = f.NamespacePrefix + f.Namespace + f.NamespaceSuffix,
            //ClassNamespaceSuffix = f.NamespaceSuffix,
            //ClassNamespacePrefix = f.NamespacePrefix,
            Usings = f.Usings,
            Interfaces = f.Interfaces,
            IsAbstract = f.IsAbstract,
            Inherit = f.Inherit,
            InheritAttribute = f.InheritAttribute,
            ToNullable = f.ToNullable,
            ClassStyle = f.ClassStyle,
            Propertes = classMembers.Where(property => property.Id == f.Id || (property.Ids ?? []).Contains(f.Id)).Select(property => new SourceGeneratorPropertyInfo
            {
                Name = property.Prefix + property.Name + property.Suffix,
                //Prefix = property.Prefix,
                //Suffix = property.Suffix,
                Modifier = property.Modifier,
                Attributes = property.Attributes,
                Remarks = property.Remarks,
                Example = property.Example,
                Summary = property.SummaryPrefix + property.Summary + property.SummarySuffix,
                //SummaryPrefix = property.SummaryPrefix,
                //SummarySuffix = property.SummarySuffix,
                IsVirtual = property.IsVirtual,
                IsOverride = property.IsOverride,
                IsNew = property.IsNew,
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
