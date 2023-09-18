///// <summary>
///// 
///// </summary>
//[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
//public abstract class AutoCodeClassBaseAttribute : Attribute
//{
//    /// <summary>
//    /// 关联id
//    /// </summary>
//    public string? Id { get; set; }
//    /// <summary>
//    /// 名称
//    /// </summary>
//    public string? Name { get; set; }
//    /// <summary>
//    /// 前缀
//    /// </summary>
//    public string? Prefix { get; set; }
//    /// <summary>
//    /// 后缀
//    /// </summary>
//    public string? Suffix { get; set; }
//    /// <summary>
//    /// 修饰符
//    /// </summary>
//    public Accessibility? Modifier { get; set; }
//    /// <summary>
//    /// 特性
//    /// </summary>
//    public string[]? Attributes { get; set; }
//    /// <summary>
//    /// 注释 描述
//    /// </summary>
//    public string? Summary { get; set; }
//    /// <summary>
//    /// 注释 示例
//    /// </summary>
//    public string? Example { get; set; }
//    /// <summary>
//    /// 注释 评论
//    /// </summary>
//    /// <remarks></remarks>
//    public string? Remarks { get; set; }
//    /// <summary>
//    /// 分部类型
//    /// </summary>
//    public bool IsPartial { get; set; }
//}

///// <summary>
///// Nullable
///// </summary>
//[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
//public sealed class AutoCodeNullableAttribute : Attribute
//{
//    public AutoCodeNullableAttribute(Nullable enable)
//    {
//        this.Enable = enable;
//    }
//    /// <summary>
//    /// 是否启用 Nullable
//    /// </summary>
//    public Nullable Enable { get; set; }
//}

///// <summary>
///// 接口
///// </summary>
//[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
//public sealed class AutoCodeClassModesAttribute : AutoCodeClassBaseAttribute
//{
//    /// <summary>
//    /// 继承
//    /// </summary>
//    public string? InheritStr { get; set; }

//    /// <summary>
//    /// 继承
//    /// </summary>
//    public Type? InheritType { get; set; }
//    /// <summary>
//    /// 接口
//    /// </summary>
//    public string[]? Interfaces { get; set; }
//    /// <summary>
//    /// 命名空间
//    /// </summary>
//    public string? Namespace { get; set; } = string.Empty;
//    /// <summary>
//    /// 命名空间前缀
//    /// </summary>
//    public string? NamespacePrefix { get; set; }
//    /// <summary>
//    /// 命名空间后缀
//    /// </summary>
//    public string? NamespaceSuffix { get; set; }
//    /// <summary>
//    /// 派生类
//    /// </summary>
//    public bool IsAbstract { get; set; }
//    /// <summary>
//    /// 命名空间引用
//    /// </summary>
//    public string[]? Usings { get; set; }
//}

///// <summary>
///// 属性
///// </summary>
//[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
//public sealed class AutoCodePropertyAttribute : AutoCodeClassBaseAttribute
//{/// <summary>
// /// 关联id
// /// </summary>
//    public string[]? Ids { get; set; }
//    /// <summary>
//    /// 类型
//    /// </summary>
//    public string? PropertyTypeStr { get; set; }
//    /// <summary>
//    /// 类型
//    /// </summary>
//    public Type? PropertyType { get; set; }
//    /// <summary>
//    /// 允许重写
//    /// </summary>
//    public bool IsVirtual { get; set; }
//    /// <summary>
//    /// 允许空
//    /// </summary>
//    public bool IsNullable { get; set; }
//    /// <summary>
//    /// 默认值
//    /// </summary>
//    public string? DefaultValue { get; set; }
//}
///// <summary>
///// 修饰符
///// </summary>
//public enum Accessibility
//{
//    /// <summary>
//    /// 没有 修饰符
//    /// </summary>
//    NotApplicable = 0,
//    /// <summary>
//    /// 访问限于包含类
//    /// </summary>
//    Private = 1,
//    /// <summary>
//    /// private protected 访问限于包含类或当前程序集中派生自包含类的类型
//    /// </summary>
//    ProtectedAndInternal = 2,
//    /// <summary>
//    /// 访问限于包含类或派生自包含类的类型
//    /// </summary>
//    Protected = 3,
//    /// <summary>
//    /// 访问限于当前程序集
//    /// </summary>
//    Internal = 4,
//    /// <summary>
//    /// protected internal 访问限于当前程序集或派生自包含类的类型
//    /// </summary>
//    ProtectedOrInternal = 5,
//    /// <summary>
//    /// 访问不受限制
//    /// </summary>
//    Public = 6
//}
///// <summary>
///// 
///// </summary>
//public enum Nullable
//{
//    /// <summary>
//    /// 将可为空注释和警告上下文设置为“已禁用”。
//    /// </summary>
//    Disable,
//    /// <summary>
//    /// 将可为空注释和警告上下文设置为“已启用”。
//    /// </summary>
//    Enable,
//    /// <summary>
//    /// 将可为空注释和警告上下文还原为项目设置。
//    /// </summary>
//    Restore,
//    /// <summary>
//    /// 将可为空注释上下文设置为“已禁用”。
//    /// </summary>
//    DisableAnnotations,
//    /// <summary>
//    /// 将可为空注释上下文设置为“已启用”。
//    /// </summary>
//    EnableAnnotations,
//    /// <summary>
//    /// 将可为空注释上下文还原为项目设置。
//    /// </summary>
//    RestoreAnnotations,
//    /// <summary>
//    /// 将可为空警告上下文设置为“已禁用”。
//    /// </summary>
//    DisableWarnings,
//    /// <summary>
//    /// 将可为空警告上下文设置为“已启用”。
//    /// </summary>
//    EnableWarnings,
//    /// <summary>
//    /// 将可为空警告上下文还原为项目设置。
//    /// </summary>
//    RestoreWarnings
//}