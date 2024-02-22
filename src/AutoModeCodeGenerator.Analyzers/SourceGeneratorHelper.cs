﻿using System.Text;

using Microsoft.CodeAnalysis;

namespace 自动代码生成;

public static class SourceGeneratorHelper
{
#pragma warning disable RS1035 // 不要使用禁用于分析器的 API
    public static Version DotNetVersion => Environment.Version;//输出.NET版本号
#pragma warning restore RS1035 // 不要使用禁用于分析器的 API
    public static string AutoGenerated => $$"""
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:{{DotNetVersion}}
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

""";

    public static string GeneratorCode(NullableEnum? nullable, params SourceGeneratorClassInfo[] generatorClassInfos)
    {
        var sourceBuilder = new StringBuilder(AutoGenerated);
        if (nullable.HasValue)
            sourceBuilder.AppendLine("#nullable " + (nullable.Value switch
            {
                NullableEnum.Enable => "enable",
                NullableEnum.Disable => "disable",
                NullableEnum.Restore => "restore",
                NullableEnum.EnableWarnings => "enable warnings",
                NullableEnum.DisableWarnings => "disable warnings",
                NullableEnum.RestoreWarnings => "restore warnings",
                NullableEnum.EnableAnnotations => "enable annotations",
                NullableEnum.DisableAnnotations => "disable annotations",
                NullableEnum.RestoreAnnotations => "restore annotations",
                _ => throw new NotImplementedException(),
            }));

        //引用
        foreach (var item in generatorClassInfos.SelectMany(f => f.Usings ?? []).Distinct().OrderBy(f => f))
            sourceBuilder.AppendLine($"using {item};");
        sourceBuilder.AppendLine();
        foreach (var namespaceInfo in generatorClassInfos.GroupBy(f => f.ClassNamespacePrefix + f.ClassNamespace + f.ClassNamespaceSuffix ?? "").OrderBy(f => f.Key).ToDictionary(f => f.Key))
        {
            if (!string.IsNullOrWhiteSpace(namespaceInfo.Key)) sourceBuilder.AppendLine($"namespace {namespaceInfo.Key}\r\n{{");
            foreach (var classInfo in namespaceInfo.Value)
            {
                sourceBuilder.AppendLine($"\t/// <summary>");
                sourceBuilder.AppendLine($"\t/// " + classInfo.SummaryPrefix + classInfo.Summary + classInfo.SummarySuffix);
                sourceBuilder.AppendLine($"\t/// </summary>");
                if (!string.IsNullOrWhiteSpace(classInfo.Example))
                {
                    sourceBuilder.AppendLine($"\t/// <example>");
                    sourceBuilder.AppendLine($"\t/// " + classInfo.Example);
                    sourceBuilder.AppendLine($"\t/// </example>");
                }
                if (!string.IsNullOrWhiteSpace(classInfo.Remarks))
                {
                    sourceBuilder.AppendLine($"\t/// <remarks>");
                    sourceBuilder.AppendLine($"\t/// " + classInfo.Remarks);
                    sourceBuilder.AppendLine($"\t/// </remarks>");
                }
                var strClassAttributes = string.Join("\r\n", classInfo.Attributes?.Select(f => "\t[" + f + "]") ?? new string[] { });
                if (!string.IsNullOrWhiteSpace(strClassAttributes)) sourceBuilder.AppendLine(strClassAttributes);
                sourceBuilder.AppendLine($"\t[System.Diagnostics.DebuggerDisplay(\"{string.Join(", ", (classInfo.Propertes ?? []).Select(f => f.Name + $" = {{{f.Name}}}"))}\")]");
                var sss100 = string.Join(", ", new[] { classInfo.Inherit }.Concat(classInfo.Interfaces ?? []).Where(f => !string.IsNullOrWhiteSpace(f)));
                var sss101 = new[] { ModifierToString(classInfo.Modifier), classInfo.IsAbstract == true ? "abstract" : "", classInfo.IsPartial == true ? "partial" : "", "class", classInfo.Prefix + classInfo.Name + classInfo.Suffix, string.IsNullOrWhiteSpace(sss100) ? null : ":", sss100 };
                sourceBuilder.AppendLine($"\t{string.Join(" ", sss101.Where(f => !string.IsNullOrWhiteSpace(f)))}\r\n\t{{");
                foreach (var item in classInfo.Propertes ?? [])
                {
                    if (new[] { item.IsVirtual, item.IsOverride, item.IsNew }.Where(f => f == true).Count() > 1) throw new Exception($"类型 {classInfo.Name} 属性 {item.Name} 的 AutoCodePropertyAttribute 特性中 IsVirtual、IsOverride、IsNew 只能一个为 true");
                    sourceBuilder.AppendLine($"\t\t/// <summary>");
                    sourceBuilder.AppendLine($"\t\t/// " + item.SummaryPrefix + item.Summary + item.SummarySuffix);
                    sourceBuilder.AppendLine($"\t\t/// </summary>");
                    if (!string.IsNullOrWhiteSpace(item.Example))
                    {
                        sourceBuilder.AppendLine($"\t\t/// <example>");
                        sourceBuilder.AppendLine($"\t\t/// " + item.Example);
                        sourceBuilder.AppendLine($"\t\t/// </example>");
                    }
                    if (!string.IsNullOrWhiteSpace(item.Remarks))
                    {
                        sourceBuilder.AppendLine($"\t\t/// <remarks>");
                        sourceBuilder.AppendLine($"\t\t/// " + item.Remarks);
                        sourceBuilder.AppendLine($"\t\t/// </remarks>");
                    }
                    var strPropertyAttributes = string.Join("\r\n", ((item.Attributes ?? []).Concat((classInfo.InheritAttribute ? item.InheritAttributes : null) ?? []))?.Select(f => "\t\t[" + f + "]") ?? new string[] { });
                    if (!string.IsNullOrWhiteSpace(strPropertyAttributes)) sourceBuilder.AppendLine(strPropertyAttributes);

                    var sss103 = new[] {
                        item.IsNew == true ? "new" : "",
                        ModifierToString(item.Modifier),
                        item.IsVirtual == true ? "virtual" : "",
                        item.IsOverride == true ? "override" : "",
                        item.Type + (item.IsNullable == true || classInfo.ToNullable ? "?" : ""),
                        item.Prefix + item.Name + item.Suffix,
                        "{ get; set; }",
                        string.IsNullOrWhiteSpace(item.DefaultValue)?null:$"= {item.DefaultValue};"
                    };
                    sourceBuilder.AppendLine($"\t\t{string.Join(" ", sss103.Where(f => !string.IsNullOrWhiteSpace(f)))}");
                }
                sourceBuilder.AppendLine($"\t}}");
            }
            if (!string.IsNullOrWhiteSpace(namespaceInfo.Key)) sourceBuilder.AppendLine($"}}");
        }
        return sourceBuilder.ToString();
    }

    private static string ModifierToString(Accessibility? accessibility)
    {
        return accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => ""
        };
    }
}