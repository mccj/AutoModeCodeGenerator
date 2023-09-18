using Microsoft.CodeAnalysis;

namespace 自动代码生成;
public class SourceGeneratorBaseInfo
{
    /// <summary>
    /// 
    /// </summary>
    public string? Name { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public Accessibility? Modifier { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? Summary { get; set; }
    public string? SummaryPrefix { get; set; }
    public string? SummarySuffix { get; set; }
    public string? Remarks { get; set; }
    public string? Example { get; set; }
}
public class SourceGeneratorClassInfo : SourceGeneratorBaseInfo
{
    public string? ClassNamespace { get; set; } = "";
    /// <summary>
    /// 
    /// </summary>
    public string[]? Usings { get; set; } = new string[] { };
    public string[]? Interfaces { get; set; }
    public bool? IsAbstract { get; set; }
    public string? Inherit { get; set; }
    public SourceGeneratorPropertyInfo[]? Propertes { get; set; }
    public string? ClassNamespacePrefix { get; set; }
    public string? ClassNamespaceSuffix { get; set; }
    public string[]? Attributes { get; set; }
    public bool? IsPartial { get; set; }
    public bool InheritAttribute { get; set; }
    public bool ToNullable { get; set; }
}
public class SourceGeneratorPropertyInfo : SourceGeneratorBaseInfo
{
    //public bool? IsReadonly { get; set; }
    public bool? IsVirtual { get; set; }
    public string? Type { get; set; }
    public string? DefaultValue { get; set; }
    public bool? IsNullable { get; set; }
    public string[]? Attributes { get; set; }
    public string[]? InheritAttributes { get; set; }
}
/// <summary>
/// 
/// </summary>
public enum NullableEnum
{
    /// <summary>
    /// 将可为空注释和警告上下文设置为“已禁用”。
    /// </summary>
    Disable,
    /// <summary>
    /// 将可为空注释和警告上下文设置为“已启用”。
    /// </summary>
    Enable,
    /// <summary>
    /// 将可为空注释和警告上下文还原为项目设置。
    /// </summary>
    Restore,
    /// <summary>
    /// 将可为空注释上下文设置为“已禁用”。
    /// </summary>
    DisableAnnotations,
    /// <summary>
    /// 将可为空注释上下文设置为“已启用”。
    /// </summary>
    EnableAnnotations,
    /// <summary>
    /// 将可为空注释上下文还原为项目设置。
    /// </summary>
    RestoreAnnotations,
    /// <summary>
    /// 将可为空警告上下文设置为“已禁用”。
    /// </summary>
    DisableWarnings,
    /// <summary>
    /// 将可为空警告上下文设置为“已启用”。
    /// </summary>
    EnableWarnings,
    /// <summary>
    /// 将可为空警告上下文还原为项目设置。
    /// </summary>
    RestoreWarnings
}