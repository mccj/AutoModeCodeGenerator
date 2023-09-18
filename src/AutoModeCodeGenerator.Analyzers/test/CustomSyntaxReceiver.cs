//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using System.Collections.Generic;

//namespace 自动代码生成.test
//{
//    /// <summary>
//    /// 语法树定义收集器，可以在这里过滤生成器所需
//    /// </summary>
//    internal class CustomSyntaxReceiver : ISyntaxReceiver
//    {
//        //BaseTypeDeclarationSyntax 
//        public List<InterfaceDeclarationSyntax> Interfaces { get; } = new();

//        /// <summary>
//        ///     访问语法树
//        /// </summary>
//        /// <param name="syntaxNode"></param>
//        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
//        {
//            //可以再此处进行过滤，如通过ClassDeclarationSyntax过滤Class类，
//            //当然也可以改为BaseTypeDeclarationSyntax,或者也可以使用InterfaceDeclarationSyntax添加接口类等等
//            if (syntaxNode is InterfaceDeclarationSyntax cds)
//            {
//                Interfaces.Add(cds);
//            }

//            if (syntaxNode is InterfaceDeclarationSyntax classDeclarationSyntax
//            && classDeclarationSyntax.AttributeLists.Count > 0)
//            {
//                //var symbol = (INamedTypeSymbol)context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

//                //if (symbol.GetAttributes().Any(a => a.AttributeClass.ToDisplayString() == "NativeObjectAttribute"))
//                //{
//                //    Interfaces.Add(symbol);
//                //}
//            }

//        }

//        //public static bool HasInterface(this ClassDeclarationSyntax source, string interfaceName)
//        //{
//        //  IEnumerable<TypeSyntax> baseTypes = source.BaseList.Types.Select(baseType => baseType.Type);
//        //    // Ideally some call to do something like...
//        //    return baseTypes.Any(baseType=>baseType.Name==interfaceName);
//        //}

  
//    }
//}