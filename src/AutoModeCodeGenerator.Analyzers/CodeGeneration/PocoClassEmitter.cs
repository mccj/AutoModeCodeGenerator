using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoModeCodeGenerator.Internal.CodeGeneration;

/// <summary>默认 ClassStyle：普通 get/set（或 init）自动属性；CRUD 主路径默认样式。</summary>
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
            includeSetter: !property.IsInit,
            GetDefaultInitializer(propertyType, property, classInfo),
            useInitSetter: property.IsInit);
    }

    protected override string? GetStyleDefaultInitializer(
        string propertyType,
        SourceGeneratorPropertyInfo property,
        SourceGeneratorClassInfo classInfo)
        => CodeGenerationHelpers.GetReferenceTypeDefaultInitializer(propertyType);
}
