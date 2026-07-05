using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace 自动代码生成.CodeGeneration;

internal abstract class ClassStyleEmitterBase : IClassStyleEmitter
{
    public abstract ClassStyleEnum Style { get; }

    public ClassDeclarationSyntax CreateClassDeclaration(SourceGeneratorClassInfo classInfo)
    {
        var members = CreateClassBodyMembers(classInfo)
            .Concat(classInfo.Properties.Select(property => CreatePropertyDeclaration(classInfo, property)))
            .ToArray();

        var debuggerDisplay = classInfo.Properties.Length > 0
            ? SyntaxGenerationHelpers.BuildDebuggerDisplay(classInfo.Properties)
            : null;

        return SyntaxFactory.ClassDeclaration(classInfo.Name!)
            .WithModifiers(SyntaxGenerationHelpers.CreateClassModifiers(classInfo))
            .WithAttributeLists(SyntaxGenerationHelpers.CreateAttributeLists(
                classInfo.Attributes,
                debuggerDisplay: debuggerDisplay))
            .WithBaseList(SyntaxGenerationHelpers.CreateBaseList(classInfo.Inherit, classInfo.Interfaces))
            .WithLeadingTrivia(SyntaxGenerationHelpers.CreateDocumentationTrivia(
                classInfo.Summary,
                classInfo.Example,
                classInfo.Remarks))
            .WithMembers(SyntaxFactory.List(members));
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
