//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.Text;
//using System.Text;
//using ClassLibrary1;

//[Generator]
//public class Log2222Generator : ISourceGenerator
//{

//    public void Initialize(GeneratorInitializationContext context)
//    {
//        context.RegisterForSyntaxNotifications(() => new CustomSyntaxReceiver());
//    }

//    public void Execute(GeneratorExecutionContext context)
//    {
//        //获取第一个附加文件内容，用作代码模板
//        var template = context.AdditionalFiles.First().GetText().ToString();

//        //获取第一个类名
//        var className = context.Compilation.SyntaxTrees.SelectMany(p => p.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()).First().Identifier.Text;

//        // 替换文本生成代码
//        // 你也可以使用模板引擎或者StringBuilder拼接出代码
//        var source = template.Replace("{Class}", className);

//        // 向编译过程添加代码文件
//        context.AddSource("Demo", SourceText.From(source, Encoding.UTF8));
//    }
//}