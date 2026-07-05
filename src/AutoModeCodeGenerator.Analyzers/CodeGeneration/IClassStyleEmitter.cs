using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace 自动代码生成.CodeGeneration;

internal interface IClassStyleEmitter
{
    ClassStyleEnum Style { get; }

    ClassDeclarationSyntax CreateClassDeclaration(SourceGeneratorClassInfo classInfo);
}
