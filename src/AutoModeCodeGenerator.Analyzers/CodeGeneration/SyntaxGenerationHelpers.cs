using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace 自动代码生成.CodeGeneration;

internal static class SyntaxGenerationHelpers
{
    public static SyntaxTriviaList CreateDocumentationTrivia(string? summary, string? example, string? remarks)
    {
        var builder = new StringBuilder();
        AppendDocSection(builder, "summary", summary);
        AppendDocSection(builder, "example", example);
        AppendDocSection(builder, "remarks", remarks);
        return builder.Length == 0
            ? default
            : SyntaxFactory.ParseLeadingTrivia(builder.ToString());
    }

    public static SyntaxList<AttributeListSyntax> CreateAttributeLists(
        IEnumerable<string> customAttributes,
        bool includeCompilerGenerated = true,
        string? debuggerDisplay = null)
    {
        var lists = new List<AttributeListSyntax>();

        foreach (var attribute in customAttributes.Where(a => !string.IsNullOrWhiteSpace(a)))
            lists.Add(ParseAttributeList(attribute));

        if (includeCompilerGenerated)
        {
            lists.Add(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.ParseName("global::System.Runtime.CompilerServices.CompilerGenerated")))));
        }

        if (!string.IsNullOrWhiteSpace(debuggerDisplay))
        {
            var display = debuggerDisplay!;
            var escapedDisplay = display.Replace("\"", "\\\"");
            lists.Add(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(
                    SyntaxFactory.ParseName("global::System.Diagnostics.DebuggerDisplay"),
                    SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.ParseExpression($"\"{escapedDisplay}\""))))))));
        }

        return lists.Count == 0 ? default : SyntaxFactory.List(lists);
    }

    public static SyntaxTokenList CreateClassModifiers(SourceGeneratorClassInfo classInfo)
    {
        var tokens = new List<SyntaxToken>();
        AddAccessibility(tokens, classInfo.Modifier);
        if (classInfo.IsAbstract == true)
            tokens.Add(SyntaxFactory.Token(SyntaxKind.AbstractKeyword));
        if (classInfo.IsPartial == true)
            tokens.Add(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
        return SyntaxFactory.TokenList(tokens);
    }

    public static SyntaxTokenList CreatePropertyModifiers(SourceGeneratorPropertyInfo property)
    {
        var tokens = new List<SyntaxToken>();
        if (property.IsNew == true)
            tokens.Add(SyntaxFactory.Token(SyntaxKind.NewKeyword));
        AddAccessibility(tokens, property.Modifier);
        if (property.IsVirtual == true)
            tokens.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
        if (property.IsOverride == true)
            tokens.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
        return SyntaxFactory.TokenList(tokens);
    }

    public static BaseListSyntax? CreateBaseList(string? inherit, ImmutableArray<string> interfaces)
    {
        var baseTypes = new[] { inherit }
            .Concat(interfaces)
            .Where(type => !string.IsNullOrWhiteSpace(type))
            .Select(type => SyntaxFactory.SimpleBaseType(ParseTypeName(type!)))
            .ToArray();

        return baseTypes.Length == 0
            ? null
            : SyntaxFactory.BaseList(SyntaxFactory.SeparatedList<BaseTypeSyntax>(baseTypes));
    }

    public static TypeSyntax ParseTypeName(string typeName)
        => SyntaxFactory.ParseTypeName(typeName);

    public static string BuildDebuggerDisplay(ImmutableArray<SourceGeneratorPropertyInfo> properties)
        => string.Join(", ", properties.Select(p => $"{p.Name} = {{{p.Name}}}"));

    public static EqualsValueClauseSyntax? CreateInitializer(string? initializerText)
    {
        if (initializerText is not { } rawText || string.IsNullOrWhiteSpace(rawText))
            return null;

        var expressionText = rawText.Trim();
        if (expressionText.StartsWith("=", StringComparison.Ordinal))
            expressionText = expressionText.Substring(1).Trim();
        if (expressionText.EndsWith(";", StringComparison.Ordinal))
            expressionText = expressionText.Substring(0, expressionText.Length - 1).Trim();

        return SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(expressionText));
    }

    public static string? GetInitializerText(
        string propertyType,
        SourceGeneratorPropertyInfo property,
        SourceGeneratorClassInfo classInfo,
        string? defaultInitializer)
    {
        if (!string.IsNullOrWhiteSpace(property.DefaultValue))
            return property.DefaultValue;

        return defaultInitializer;
    }

    public static PropertyDeclarationSyntax CreateSimpleAutoProperty(
        SourceGeneratorClassInfo classInfo,
        SourceGeneratorPropertyInfo property,
        bool includeSetter,
        string? initializerText)
    {
        var propertyType = CodeGenerationHelpers.GetPropertyType(property, classInfo);
        var accessors = new List<AccessorDeclarationSyntax>
        {
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
        };

        if (includeSetter)
        {
            accessors.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }

        var declaration = SyntaxFactory.PropertyDeclaration(ParseTypeName(propertyType), property.Name!)
            .WithModifiers(CreatePropertyModifiers(property))
            .WithAttributeLists(CreateAttributeLists(CodeGenerationHelpers.GetPropertyAttributes(property, classInfo), includeCompilerGenerated: false))
            .WithLeadingTrivia(CreateDocumentationTrivia(property.Summary, property.Example, property.Remarks))
            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(accessors)));

        var initializer = CreateInitializer(initializerText);
        if (initializer is null)
            return declaration;

        return declaration
            .WithInitializer(initializer)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    public static MemberDeclarationSyntax ParseMember(string memberDeclaration)
        => SyntaxFactory.ParseMemberDeclaration(memberDeclaration)
            ?? throw new InvalidOperationException($"无法解析成员声明: {memberDeclaration}");

    public static string FormatCompilationUnit(CompilationUnitSyntax compilationUnit)
        => compilationUnit
            .NormalizeWhitespace(eol: "\r\n")
            .GetText(Encoding.UTF8)
            .ToString();

    private static AttributeListSyntax ParseAttributeList(string attributeText)
    {
        var member = SyntaxFactory.ParseMemberDeclaration("[" + attributeText + "]\r\nclass __Dummy { }") as ClassDeclarationSyntax
            ?? throw new InvalidOperationException($"无法解析特性: {attributeText}");
        return member.AttributeLists[0];
    }

    private static void AppendDocSection(StringBuilder builder, string tag, string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return;

        builder.AppendLine($"/// <{tag}>");
        foreach (var line in SplitLines(content!))
            builder.AppendLine($"/// {line}");
        builder.AppendLine($"/// </{tag}>");
    }

    private static IEnumerable<string> SplitLines(string content)
        => content.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(line => line.Trim());

    private static void AddAccessibility(List<SyntaxToken> tokens, Accessibility? accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Public:
                tokens.Add(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                break;
            case Accessibility.Private:
                tokens.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                break;
            case Accessibility.Internal:
                tokens.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case Accessibility.Protected:
                tokens.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
            case Accessibility.ProtectedOrInternal:
                tokens.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                tokens.Add(SyntaxFactory.Token(SyntaxKind.InternalKeyword));
                break;
            case Accessibility.ProtectedAndInternal:
                tokens.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                tokens.Add(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword));
                break;
        }
    }
}
