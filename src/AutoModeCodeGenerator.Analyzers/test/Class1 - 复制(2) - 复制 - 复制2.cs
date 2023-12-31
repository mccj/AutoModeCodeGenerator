﻿//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Text;

//namespace ClassLibrary1
//{
//    [Generator]
//    public class AutoNotifyGenerator : ISourceGenerator
//    {
//        private const string attributeText = @"
//using System;
//namespace AutoNotify
//{
//    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
//    sealed class AutoNotifyAttribute : Attribute
//    {
//        public AutoNotifyAttribute()
//        {
//        }
//        public string PropertyName { get; set; }
//    }
//}
//";

//        public void Initialize(GeneratorInitializationContext context)
//        {
//            // 注册一个语法接收器，会在每次生成时被创建
//            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
//        }

//        public void Execute(GeneratorExecutionContext context)
//        {
//            // 添加 Attrbite 文本
//            context.AddSource("AutoNotifyAttribute", SourceText.From(attributeText, Encoding.UTF8));

//            // 获取先前的语法接收器 
//            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
//                return;

//            // 创建处目标名称的属性
//            CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
//            Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));

//            // 获取新绑定的 Attribute，并获取INotifyPropertyChanged
//            INamedTypeSymbol attributeSymbol = compilation.GetTypeByMetadataName("AutoNotify.AutoNotifyAttribute");
//            INamedTypeSymbol notifySymbol = compilation.GetTypeByMetadataName("System.ComponentModel.INotifyPropertyChanged");

//            // 遍历字段，只保留有 AutoNotify 标注的字段
//            List<IFieldSymbol> fieldSymbols = new List<IFieldSymbol>();
//            foreach (FieldDeclarationSyntax field in receiver.CandidateFields)
//            {
//                SemanticModel model = compilation.GetSemanticModel(field.SyntaxTree);
//                foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
//                {
//                    // 获取字段符号信息，如果有 AutoNotify 标注则保存
//                    IFieldSymbol fieldSymbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;
//                    if (fieldSymbol.GetAttributes().Any(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
//                    {
//                        fieldSymbols.Add(fieldSymbol);
//                    }
//                }
//            }

//            // 按 class 对字段进行分组，并生成代码
//            foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group in fieldSymbols.GroupBy(f => f.ContainingType))
//            {
//                string classSource = ProcessClass(group.Key, group.ToList(), attributeSymbol, notifySymbol, context);
//                context.AddSource($"{group.Key.Name}_autoNotify.cs", SourceText.From(classSource, Encoding.UTF8));
//            }
//        }

//        private string ProcessClass(INamedTypeSymbol classSymbol, List<IFieldSymbol> fields, ISymbol attributeSymbol, ISymbol notifySymbol, SourceGeneratorContext context)
//        {
//            if (!classSymbol.ContainingSymbol.Equals(classSymbol.ContainingNamespace, SymbolEqualityComparer.Default))
//            {
//                // TODO: 必须在顶层，产生诊断信息
//                return null;
//            }

//            string namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

//            // 开始构建要生成的代码
//            StringBuilder source = new StringBuilder($@"
//namespace {namespaceName}
//{{
//    public partial class {classSymbol.Name} : {notifySymbol.ToDisplayString()}
//    {{
//");

//            // 如果类型还没有实现 INotifyPropertyChanged 则添加实现
//            if (!classSymbol.Interfaces.Contains(notifySymbol))
//            {
//                source.Append("public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;");
//            }

//            // 生成属性
//            foreach (IFieldSymbol fieldSymbol in fields)
//            {
//                ProcessField(source, fieldSymbol, attributeSymbol);
//            }

//            source.Append("} }");
//            return source.ToString();
//        }

//        private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol attributeSymbol)
//        {
//            // 获取字段名称
//            string fieldName = fieldSymbol.Name;
//            ITypeSymbol fieldType = fieldSymbol.Type;

//            // 获取 AutoNotify Attribute 和相关的数据
//            AttributeData attributeData = fieldSymbol.GetAttributes().Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));
//            TypedConstant overridenNameOpt = attributeData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "PropertyName").Value;

//            string propertyName = chooseName(fieldName, overridenNameOpt);
//            if (propertyName.Length == 0 || propertyName == fieldName)
//            {
//                //TODO: 无法处理，产生诊断信息
//                return;
//            }

//            source.Append($@"
//public {fieldType} {propertyName} 
//{{
//    get 
//    {{
//        return this.{fieldName};
//    }}
//    set
//    {{
//        this.{fieldName} = value;
//        this.PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof({propertyName})));
//    }}
//}}
//");

//            string chooseName(string fieldName, TypedConstant overridenNameOpt)
//            {
//                if (!overridenNameOpt.IsNull)
//                {
//                    return overridenNameOpt.Value.ToString();
//                }

//                fieldName = fieldName.TrimStart('_');
//                if (fieldName.Length == 0)
//                    return string.Empty;

//                if (fieldName.Length == 1)
//                    return fieldName.ToUpper();

//                return fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);
//            }

//        }

//        // 语法接收器，将在每次生成代码时被按需创建
//        class SyntaxReceiver : ISyntaxReceiver
//        {
//            public List<FieldDeclarationSyntax> CandidateFields { get; } = new List<FieldDeclarationSyntax>();

//            // 编译中在访问每个语法节点时被调用，我们可以检查节点并保存任何对生成有用的信息
//            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
//            {
//                // 将具有至少一个 Attribute 的任何字段作为候选
//                if (syntaxNode is FieldDeclarationSyntax fieldDeclarationSyntax
//                    && fieldDeclarationSyntax.AttributeLists.Count > 0)
//                {
//                    CandidateFields.Add(fieldDeclarationSyntax);
//                }
//            }
//        }
//    }
//}