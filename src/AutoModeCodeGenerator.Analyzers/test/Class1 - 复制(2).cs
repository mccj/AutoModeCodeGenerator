//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System.Collections.Immutable;

//namespace SourceGenerator;

//[Generator]
//public class TypeWithAttributeGenerator : IIncrementalGenerator
//{
//    /// <summary>
//    /// 对拥有某attribute的type生成代码
//    /// </summary>
//    /// <param name="typeDeclarationSyntax"></param>
//    /// <param name="typeSymbol"></param>
//    /// <param name="attributeList">该类的某种Attribute</param>
//    /// <returns>生成的代码</returns>
//    private delegate string? TypeWithAttribute(TypeDeclarationSyntax typeDeclarationSyntax, INamedTypeSymbol typeSymbol, List<AttributeData> attributeList);

//    /// <summary>
//    /// 需要生成的Attribute
//    /// </summary>
//    private static readonly Dictionary<string, TypeWithAttribute> Attributes = new()
//    {
//        { "Attributes.GenerateConstructorAttribute", TypeWithAttributeDelegates.GenerateConstructor },
//        { "Attributes.LoadSaveConfigurationAttribute", TypeWithAttributeDelegates.LoadSaveConfiguration },
//        { "Attributes.DependencyPropertyAttribute", TypeWithAttributeDelegates.DependencyProperty }
//    };

//    public void Initialize(IncrementalGeneratorInitializationContext context);

//    private static bool IsSyntaxTargetForGeneration(SyntaxNode node);

//    private static TypeDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context);

//    private static void Execute(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> types, SourceProductionContext context);
//}
