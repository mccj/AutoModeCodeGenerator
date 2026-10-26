using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoModeCodeGenerator.Internal.CodeGeneration;

internal abstract class ClassStyleEmitterBase : IClassStyleEmitter
{
    public abstract ClassStyleEnum Style { get; }

    public TypeDeclarationSyntax CreateTypeDeclaration(SourceGeneratorClassInfo classInfo)
    {
        var members = CreateClassBodyMembers(classInfo)
            .Concat(classInfo.Properties.Select(property => CreatePropertyDeclaration(classInfo, property)))
            .ToArray();

        var debuggerDisplay = classInfo.Properties.Length > 0
            ? SyntaxGenerationHelpers.BuildDebuggerDisplay(classInfo.Properties)
            : null;

        var attributeLists = SyntaxGenerationHelpers.CreateAttributeLists(
            classInfo.Attributes,
            debuggerDisplay: debuggerDisplay);
        var baseList = SyntaxGenerationHelpers.CreateBaseList(classInfo.Inherit, classInfo.Interfaces);
        var leadingTrivia = SyntaxGenerationHelpers.CreateDocumentationTrivia(
            classInfo.Summary,
            classInfo.Example,
            classInfo.Remarks);
        var modifiers = SyntaxGenerationHelpers.CreateClassModifiers(classInfo);
        var memberList = SyntaxFactory.List(members);

        if (Style == ClassStyleEnum.Record)
        {
            return SyntaxFactory.RecordDeclaration(
                    SyntaxFactory.Token(SyntaxKind.RecordKeyword),
                    classInfo.Name!)
                .WithClassOrStructKeyword(SyntaxFactory.Token(SyntaxKind.ClassKeyword))
                .WithModifiers(modifiers)
                .WithAttributeLists(attributeLists)
                .WithBaseList(baseList)
                .WithLeadingTrivia(leadingTrivia)
                .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                .WithMembers(memberList)
                .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
        }

        return SyntaxFactory.ClassDeclaration(classInfo.Name!)
            .WithModifiers(modifiers)
            .WithAttributeLists(attributeLists)
            .WithBaseList(baseList)
            .WithLeadingTrivia(leadingTrivia)
            .WithMembers(memberList);
    }

    protected virtual IEnumerable<MemberDeclarationSyntax> CreateClassBodyMembers(SourceGeneratorClassInfo classInfo)
        => [];

    protected abstract MemberDeclarationSyntax CreatePropertyDeclaration(
        SourceGeneratorClassInfo classInfo,
        SourceGeneratorPropertyInfo property);

    protected string? GetDefaultInitializer(
        string propertyType,
        SourceGeneratorPropertyInfo property,
        SourceGeneratorClassInfo classInfo)
        => SyntaxGenerationHelpers.GetInitializerText(
            propertyType,
            property,
            classInfo,
            GetStyleDefaultInitializer(propertyType, property, classInfo));

    protected abstract string? GetStyleDefaultInitializer(
        string propertyType,
        SourceGeneratorPropertyInfo property,
        SourceGeneratorClassInfo classInfo);
}
