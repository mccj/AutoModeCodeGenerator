using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoModeCodeGenerator.Internal.CodeGeneration;

internal interface IClassStyleEmitter
{
    ClassStyleEnum Style { get; }

    TypeDeclarationSyntax CreateTypeDeclaration(SourceGeneratorClassInfo classInfo);
}
