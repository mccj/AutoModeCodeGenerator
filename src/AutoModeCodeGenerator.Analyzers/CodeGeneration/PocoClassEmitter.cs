using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace 自动代码生成.CodeGeneration;

internal sealed class PocoClassEmitter : ClassStyleEmitterBase
{
    public override ClassStyleEnum Style => ClassStyleEnum.Poco;

    protected override MemberDeclarationSyntax CreatePropertyDeclaration(
        SourceGeneratorClassInfo classInfo,
        SourceGeneratorPropertyInfo property)
    {
        var propertyType = CodeGenerationHelpers.GetPropertyType(property, classInfo);
        return SyntaxGenerationHelpers.CreateSimpleAutoProperty(
            classInfo,
            property,
            includeSetter: true,
            GetDefaultInitializer(propertyType, property, classInfo));
    }

    protected override string? GetStyleDefaultInitializer(
        string propertyType,
        SourceGeneratorPropertyInfo property,
        SourceGeneratorClassInfo classInfo)
        => CodeGenerationHelpers.GetReferenceTypeDefaultInitializer(propertyType);
}
