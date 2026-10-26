using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoModeCodeGenerator.Internal.CodeGeneration;

/// <summary>ClassStyle.Inpc：INotifyPropertyChanged 字段式属性；特性与 entity.json 均可选用。</summary>
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

        // SetValue 内部比较并触发 PropertyChanged，此处不再重复 RaisePropertyChanged。
        var setAccessor = SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
            .WithBody(SyntaxFactory.Block(
                SyntaxFactory.IfStatement(
                    compareExpression,
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("SetValue(value)"))))));

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
