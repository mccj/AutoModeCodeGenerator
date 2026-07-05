using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace 自动代码生成.CodeGeneration;

internal sealed class RecordClassEmitter : ClassStyleEmitterBase
{
    public override ClassStyleEnum Style => ClassStyleEnum.Record;

    protected override IEnumerable<MemberDeclarationSyntax> CreateClassBodyMembers(SourceGeneratorClassInfo classInfo)
    {
        if (classInfo.Properties.Length == 0)
            yield break;

        var parameters = SyntaxFactory.SeparatedList(
            classInfo.Properties.Select(property =>
            {
                var parameterName = "@" + CodeGenerationHelpers.LowerFirstLetter(property.Name);
                return SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameterName))
                    .WithType(SyntaxGenerationHelpers.ParseTypeName(
                        CodeGenerationHelpers.GetPropertyType(property, classInfo)));
            }));

        var statements = classInfo.Properties.Select(property =>
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.ThisExpression(),
                        SyntaxFactory.IdentifierName(property.Name!)),
                    SyntaxFactory.IdentifierName("@" + CodeGenerationHelpers.LowerFirstLetter(property.Name)))));

        yield return SyntaxFactory.ConstructorDeclaration(classInfo.Name!)
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
            .WithParameterList(SyntaxFactory.ParameterList(parameters))
            .WithBody(SyntaxFactory.Block(statements));
    }

    protected override MemberDeclarationSyntax CreatePropertyDeclaration(
        SourceGeneratorClassInfo classInfo,
        SourceGeneratorPropertyInfo property)
        => SyntaxGenerationHelpers.CreateSimpleAutoProperty(
            classInfo,
            property,
            includeSetter: false,
            initializerText: null);

    protected override string? GetStyleDefaultInitializer(
        string propertyType,
        SourceGeneratorPropertyInfo property,
        SourceGeneratorClassInfo classInfo)
        => null;
}
