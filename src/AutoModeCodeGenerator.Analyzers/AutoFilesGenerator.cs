//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Text;

//using System.Diagnostics;
//using System.Text;

////namespace 自动代码生成;

//[Generator(LanguageNames.CSharp)]
//public partial class AutoFilesGenerator : IIncrementalGenerator
//{
//    public void Initialize(IncrementalGeneratorInitializationContext context)
//    {
//        //对于Source Generator可以通过添加`Debugger.Launch()`的形式进行对编译时的生成器进行调试，可以通过它很便捷的一步步调试代码.
//        //Debugger.Launch();

//        try
//        {
//            var compilation = context.CompilationProvider;

//            var entityFileIncrementalValuesProvider = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".entity.json", StringComparison.OrdinalIgnoreCase));
//            var combined = entityFileIncrementalValuesProvider.Combine(compilation);
//            context.RegisterSourceOutput(combined, (ctx, result) =>
//            {
//                //Debugger.Launch();
//                var csgSource = result.Left.GetText();
//                if (csgSource == null) return;

//                //var code = SourceGeneratorHelper.GeneratorCode(sources);
//                //ctx.AddSource(Path.GetFileNameWithoutExtension(result.Left.Path) + ".g.cs", SourceText.From(csgSource.ToString(), Encoding.UTF8));
//            });



//            //var incrementalValuesProvider = context.SyntaxProvider.CreateSyntaxProvider((syntaxNode, _) =>
//            //{
//            //    // 在此进行快速的语法判断逻辑，可以判断当前的内容是否感兴趣，如此过滤掉一些内容，从而减少后续处理，提升性能
//            //    // 这里样式的是获取到 Program 类的完全限定名，也就是只需要用到 Class 类型
//            //    //return syntaxNode.IsKind(SyntaxKind.ClassDeclaration);
//            //    return true;
//            //},
//            //    (generatorSyntaxContext, _) =>
//            //    {
//            //        Debugger.Launch();
//            //        // 从这里可以获取到语法内容
//            //        if (generatorSyntaxContext.Node is ClassDeclarationSyntax classDeclarationSyntax)
//            //        {
//            //            var symbolInfo = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;
//            //            // 带上 global 格式的输出 FullName 内容
//            //            var symbolDisplayFormat = new SymbolDisplayFormat
//            //            (
//            //                // 带上命名空间和类型名
//            //                SymbolDisplayGlobalNamespaceStyle.Included,
//            //                // 命名空间之前加上 global 防止冲突
//            //                SymbolDisplayTypeQualificationStyle
//            //                    .NameAndContainingTypesAndNamespaces
//            //            );
//            //            var displayString = symbolInfo.ToDisplayString(symbolDisplayFormat);
//            //        }
//            //        else
//            //        {
//            //            // 理论上不会进入此分支，因为在之前判断了类型
//            //        }

//            //        return generatorSyntaxContext;
//            //    });

//            //context.RegisterSourceOutput(incrementalValuesProvider, (sourceProductionContext, generatorSyntaxContext) =>
//            //{
//            //    // 加上这里只是为了让 incrementalValuesProvider 能够执行
//            //});
//        }
//        catch (Exception ex)
//        {
//            Debug.Fail(ex.Message);
//        }
//    }
//}
