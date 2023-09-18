using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace 自动代码生成.test
{
    /// <summary>
    /// https://andrewlock.net/series/creating-a-source-generator/
    /// </summary>
    [Generator]
    internal class AutoCodeGenerator : IIncrementalGenerator //ISourceGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext initContext)
        {
            //对于Source Generator可以通过添加`Debugger.Launch()`的形式进行对编译时的生成器进行调试，可以通过它很便捷的一步步调试代码.
            Debugger.Launch();


            // 获取到所有引用程序集
            var referencedAssemblySymbols = initContext.CompilationProvider.SelectMany((compilation, cancellationToken) =>
            {
                //var sss = compilation.GetTypeByMetadataName("Point2");
                //var mainMethod = compilation.GetEntryPoint(cancellationToken);
                var className = compilation.SyntaxTrees.SelectMany(p => p.GetRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>()).First().Identifier.Text;

                //// 创建处目标名称的属性
                //var options = (compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
                ////Compilation compilation1 = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));


                return compilation.SourceModule.ReferencedAssemblySymbols;
            });

            var incrementalValuesProvider = initContext.SyntaxProvider.CreateSyntaxProvider((syntaxNode, cancellationToken) =>
            {
                return syntaxNode.IsKind(SyntaxKind.InterfaceDeclaration);
            }, (GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken) =>
            {           // 从这里可以获取到语法内容
                if (generatorSyntaxContext.Node is InterfaceDeclarationSyntax classDeclarationSyntax)
                {
                    var symbolInfo = generatorSyntaxContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax)!;
                    // 带上 global 格式的输出 FullName 内容
                    var symbolDisplayFormat = new SymbolDisplayFormat
                    (
                        // 带上命名空间和类型名
                        SymbolDisplayGlobalNamespaceStyle.Included,
                        // 命名空间之前加上 global 防止冲突
                        SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                    );
                    var displayString = symbolInfo.ToDisplayString(symbolDisplayFormat);
                }
                else
                {
                    // 理论上不会进入此分支，因为在之前判断了类型
                }

                return generatorSyntaxContext;
            });

            initContext.RegisterSourceOutput(incrementalValuesProvider, (sourceProductionContext, compilation) =>
            {
            });
            //            // 找到对什么文件感兴趣
            //             var typeNameIncrementalValueProvider = initContext.CompilationProvider.Select((compilation, cancellationToken) =>
            //            {
            //                // 获取到所有引用程序集
            //                var referencedAssemblySymbols = compilation.SourceModule.ReferencedAssemblySymbols;

            //                // 为了方便代码理解，这里只取名为 Lib 程序集的内容…
            //                foreach (var referencedAssemblySymbol in referencedAssemblySymbols)
            //                {
            //                    var name = referencedAssemblySymbol.Name;

            //                    if (name.Contains("Lib"))
            //                    {
            //                        // 在这里编写获取程序集所有类型的代码
            //                    }
            //                    else
            //                    {
            //                        // 其他的引用程序集，在这里就忽略
            //                    }
            //                }
            //                // 忽略代码
            //                return compilation;
            //            });

            //            initContext.RegisterSourceOutput(typeNameIncrementalValueProvider, (sourceProductionContext, compilation) =>
            //            {
            //                var syntaxTree = compilation.SyntaxTrees.FirstOrDefault();
            //                if (syntaxTree == null)
            //                {
            //                    return;
            //                }

            //                var root = syntaxTree.GetRoot(sourceProductionContext.CancellationToken);

            //                // 选择给 Program 的附加上
            //                var classDeclarationSyntax = root
            //                    .DescendantNodes(descendIntoTrivia: true)
            //                    .OfType<ClassDeclarationSyntax>()
            //                    .FirstOrDefault();
            //                if (classDeclarationSyntax?.Identifier.Text != "Program")
            //                {
            //                    // 如果变更的非预期类型，那就不加上代码，否则代码将会重复加入
            //                    return;
            //                }

            //                // 这是一个很强的技术，在代码没有变更的情况下，多次构建，是可以看到不会重复进入此逻辑，也就是 Count 属性没有加一
            //                // 可以试试对一个大的项目，修改部分代码，看看 Count 属性

            //                string source = $@"
            //using System;

            //namespace WhacadenaKewarfellaja
            //{{
            //    public static partial class Program
            //    {{
            //        static partial void HelloFrom(string name)
            //        {{
            //            Console.WriteLine($""构建 {Count} 次 says: Hi from '{{name}}'"");
            //        }}
            //    }}
            //}}
            //";
            //                sourceProductionContext.AddSource("GeneratedSourceTest", source);

            //                Count++;
            //            });
            //// Add the marker attribute to the compilation
            //initContext.RegisterPostInitializationOutput(ctx => ctx.AddSource("EnumExtensionsAttribute.g.cs", SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

            //// Do a simple filter for enums
            //IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations = initContext.SyntaxProvider
            //    .CreateSyntaxProvider(
            //        predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select enums with attributes
            //        transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
            //    .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

            //// Combine the selected enums with the `Compilation`
            //IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
            //    = initContext.CompilationProvider.Combine(enumDeclarations.Collect());

            //// Generate the source using the compilation and enums
            //initContext.RegisterSourceOutput(compilationAndEnums,
            //    static (spc, source) => Execute(source.Item1, source.Item2, spc));


            // define the execution pipeline here via a series of transformations:

            //        // find all additional files that end with .txt
            //        IncrementalValuesProvider<AdditionalText> textFiles = initContext.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".cs"));

            //        // read their contents and save their name
            //        IncrementalValuesProvider<(string name, string content)> namesAndContents = textFiles.Select((text, cancellationToken) => (name: Path.GetFileNameWithoutExtension(text.Path), content: text.GetText(cancellationToken)!.ToString()));

            //        // generate a class that contains their values as const strings
            //        initContext.RegisterSourceOutput(namesAndContents, (spc, nameAndContent) =>
            //        {
            //            spc.AddSource($"ConstStrings.{nameAndContent.name}", $@"
            //public static partial class ConstStrings
            //{{
            //    public const string {nameAndContent.name} = ""{nameAndContent.content}"";
            //}}");
            //        });


            //// get the additional text provider
            //IncrementalValuesProvider<AdditionalText> additionalTexts = initContext.AdditionalTextsProvider;

            //// apply a 1-to-1 transform on each text, which represents extracting the path
            //IncrementalValuesProvider<string> transformed = additionalTexts.Select(static (text, _) => text.Path);

            //// transform each extracted path into something else
            //IncrementalValuesProvider<string> prefixTransform = transformed.Select(static (path, _) => "prefix_" + path);

            // get the additional text provider

            //// extract each element from each additional file
            //IncrementalValuesProvider<MyElementType> elements = additionalTexts.SelectMany(static (text, _) => /*transform text into an array of MyElementType*/);
            //// now the generator can consider the union of elements in all additional texts, without needing to consider multiple files
            //IncrementalValuesProvider<string> transformed = elements.Select(static (element, _) => /*transform the individual element*/);


            //// filter additional texts by extension
            //IncrementalValuesProvider<string> xmlFiles = additionalTexts.Where(static (text, _) => text.Path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));


            //// collect the additional texts into a single item
            //var collected = additionalTexts.Collect();

            //// perform a transformation where you can access all texts at once
            //var transform = collected.Select(static (texts, _) => /* ... */);

            //// combine each additional text with the parse options
            //IncrementalValuesProvider<(AdditionalText, ParseOptions)> combined = additionalTexts.Combine(initContext.ParseOptionsProvider);

            //// perform a transform on each text, with access to the options
            //var transformed = combined.Select(static (pair, _) =>
            //{
            //    AdditionalText text = pair.Left;
            //    ParseOptions parseOptions = pair.Right;
            //    // do the actual transform ...
            //});

            //var returnKinds = initContext.SyntaxProvider.CreateSyntaxProvider(static (n, _) => n is MethodDeclarationSyntax,
            //                                                      static (n, _) => ((IMethodSymbol)n.SemanticModel.GetDeclaredSymbol(n.Node)).ReturnType.Kind);


            //            // get the additional text provider
            //            IncrementalValuesProvider<AdditionalText> additionalTexts = initContext.AdditionalTextsProvider;

            //            // apply a 1-to-1 transform on each text, extracting the path
            //            IncrementalValuesProvider<string> transformed = additionalTexts.Select(static (text, _) => text.Path);

            //            // collect the paths into a batch
            //            IncrementalValueProvider<ImmutableArray<string>> collected = transformed.Collect();

            //            // take the file paths from the above batch and make some user visible syntax
            //            initContext.RegisterSourceOutput(collected, static (sourceProductionContext, filePaths) =>
            //            {
            //                sourceProductionContext.AddSource("additionalFiles.cs", @"
            //namespace Generated
            //{
            //    public class AdditionalTextList
            //    {
            //        public static void PrintTexts()
            //        {
            //            System.Console.WriteLine(""Additional Texts were: " + string.Join(", ", filePaths) + @" "");
            //        }
            //    }
            //}");
            //            });


            initContext.RegisterImplementationSourceOutput(initContext.AnalyzerConfigOptionsProvider,
                (productionContext, provider) =>
                {
                    var text = string.Empty;

                    // 通过 csproj 等 PropertyGroup 里面获取
                    // 需要将可见的，放入到 CompilerVisibleProperty 里面
                    // 需要加上 `build_property.` 前缀
                    if (provider.GlobalOptions.TryGetValue("build_property.MyCustomProperty", out var myCustomProperty))
                    {
                        text += " " + myCustomProperty;
                    }

                    var code = @"using System;
namespace LainewihereJerejawwerye
{
    public static class Foo
    {
        public static void F1()
        {
            Console.WriteLine(""" + text + @""");
        }
    }
}";
                    productionContext.AddSource("Demo", code);
                });

            var compilation = initContext.CompilationProvider;
            var texts = initContext.AdditionalTextsProvider;

            // Don't do this!
            var combined = texts.Combine(compilation);

            initContext.RegisterSourceOutput(combined, static (spc, pair) =>
            {
                var assemblyName = pair.Right.AssemblyName;
                // produce source ...
            });
        }
        private static int Count { set; get; } = 0;

    }
}
