using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace 自动代码生成.CodeGeneration;

internal sealed class InpcClassEmitter : ClassStyleEmitterBase
{
    public override ClassStyleEnum Style => ClassStyleEnum.Inpc;

    protected override MemberDeclarationSyntax CreatePropertyDeclaration(
        SourceGeneratorClassInfo classInfo,
        SourceGeneratorPropertyInfo property)
    {
        var propertyType = CodeGenerationHelpers.GetPropertyType(property, classInfo);
        var typeSyntax = SyntaxGenerationHelpers.ParseTypeName(propertyType);
        var getValueExpression = SyntaxFactory.ParseExpression($"base.GetValue<{propertyType}>(default)");
        var compareExpression = SyntaxFactory.ParseExpression($"GetValue<{propertyType}>(default) != value");

        var getAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(getValueExpression)));

        var setAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    compareExpression,
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("SetValue(value)")),
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("RaisePropertyChanged()"))))));

        var declaration = SyntaxFactory.PropertyDeclaration(typeSyntax, property.Name!)
            .WithModifiers(SyntaxGenerationHelpers.CreatePropertyModifiers(property))
            .WithAttributeLists(SyntaxGenerationHelpers.CreateAttributeLists(
                CodeGenerationHelpers.GetPropertyAttributes(property, classInfo),
                includeCompilerGenerated: false))
            .WithLeadingTrivia(SyntaxGenerationHelpers.CreateDocumentationTrivia(
                property.Summary,
                property.Example,
                property.Remarks))
            .WithAccessorList(SyntaxFactory.AccessorList(SyntaxFactory.List(new[] { getAccessor, setAccessor })));

        var initializer = GetDefaultInitializer(propertyType, property, classInfo);
        var initializerClause = SyntaxGenerationHelpers.CreateInitializer(initializer);
        if (initializerClause is null)
            return declaration;

        return declaration
            .WithInitializer(initializerClause)
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
    }

    protected override string? GetStyleDefaultInitializer(
        string propertyType,
        SourceGeneratorPropertyInfo property,
        SourceGeneratorClassInfo classInfo)
        => CodeGenerationHelpers.GetReferenceTypeDefaultInitializer(propertyType);
}
