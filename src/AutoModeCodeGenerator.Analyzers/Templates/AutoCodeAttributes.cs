#nullable enable
using System;
namespace AutoCodeGenerator
{
    /// <summary>
    /// 自动生成特性的公共基类，供类模式与属性特性共享名称、修饰符、文档等配置。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public abstract class AutoCodeClassBaseAttribute : Attribute
    {
        /// <summary>关联的模式 Id，与 <see cref="AutoCodePropertyAttribute.Id"/> / <see cref="AutoCodePropertyAttribute.Ids"/> 对应。</summary>
        public string? Id { get; set; }
        /// <summary>生成成员的名称；未指定时使用源类型或源属性名。</summary>
        public string? Name { get; set; }
        /// <summary>生成名称的前缀。</summary>
        public string? Prefix { get; set; }
        /// <summary>生成名称的后缀。</summary>
        public string? Suffix { get; set; }
        /// <summary>访问修饰符。</summary>
        public AccessibilityEnum Modifier { get; set; }
        /// <summary>附加到生成成员上的特性字符串（如 <c>global::System.Serializable</c>）。</summary>
        public string[]? Attributes { get; set; }
        /// <summary>XML 文档 &lt;summary&gt; 正文。</summary>
        public string? Summary { get; set; }
        /// <summary>拼接到 &lt;summary&gt; 前的文本。</summary>
        public string? SummaryPrefix { get; set; }
        /// <summary>拼接到 &lt;summary&gt; 后的文本。</summary>
        public string? SummarySuffix { get; set; }
        /// <summary>XML 文档 &lt;example&gt; 正文。</summary>
        public string? Example { get; set; }
        /// <summary>XML 文档 &lt;remarks&gt; 正文。</summary>
        public string? Remarks { get; set; }
        /// <summary>是否生成 <c>partial</c> 类型，默认 <c>true</c>。</summary>
        public bool IsPartial { get; set; }
    }

    /// <summary>
    /// 控制生成文件顶部的 <c>#nullable</c> 指令。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AutoCodeNullableAttribute : Attribute
    {
        /// <summary>可为空上下文类型，见 <see cref="NullableEnum"/>。</summary>
        public NullableEnum Type { get; set; }
    }

    /// <summary>
    /// 在接口或类上声明一种代码生成模式（命名空间、样式、基类等）。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class AutoCodeClassModesAttribute : AutoCodeClassBaseAttribute
    {
        /// <summary>基类类型名（字符串形式）。</summary>
        public string? InheritStr { get; set; }
        /// <summary>基类类型（<see cref="Type"/> 形式）。</summary>
        public Type? InheritType { get; set; }
        /// <summary>实现的接口类型名列表（字符串形式）。</summary>
        public string[]? InterfaceStrs { get; set; }
        /// <summary>实现的接口类型列表（<see cref="Type"/> 形式）。</summary>
        public Type[]? InterfaceTypes { get; set; }
        /// <summary>生成类的目标命名空间。</summary>
        public string? Namespace { get; set; } = string.Empty;
        /// <summary>命名空间前缀。</summary>
        public string? NamespacePrefix { get; set; }
        /// <summary>命名空间后缀。</summary>
        public string? NamespaceSuffix { get; set; }
        /// <summary>是否生成 <c>abstract</c> 类。</summary>
        public bool IsAbstract { get; set; }
        /// <summary>生成文件顶部附加的 <c>using</c> 指令。</summary>
        public string[]? Usings { get; set; }
        /// <summary>是否将源属性上的外部特性复制到生成属性。</summary>
        public bool InheritAttribute { get; set; }
        /// <summary>为 <c>true</c> 时，所有生成属性类型均标记为可空。</summary>
        public bool ToNullable { get; set; }
        /// <summary>生成类的代码样式（Poco / Record / Inpc）。</summary>
        public ClassStyleEnum ClassStyle { get; set; }
    }

    /// <summary>
    /// 在属性上声明其参与哪些生成模式，并可覆盖类型、修饰符等。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class AutoCodePropertyAttribute : AutoCodeClassBaseAttribute
    {
        /// <summary>关联的多个模式 Id（与 <see cref="AutoCodeClassBaseAttribute.Id"/> 二选一或并用）。</summary>
        public string[]? Ids { get; set; }
        /// <summary>覆盖属性类型（字符串形式）。</summary>
        public string? PropertyTypeStr { get; set; }
        /// <summary>覆盖属性类型（<see cref="Type"/> 形式）。</summary>
        public Type? PropertyType { get; set; }
        /// <summary>是否生成 <c>virtual</c> 属性。</summary>
        public bool IsVirtual { get; set; }
        /// <summary>是否生成 <c>override</c> 属性。</summary>
        public bool IsOverride { get; set; }
        /// <summary>是否生成 <c>new</c> 隐藏属性。</summary>
        public bool IsNew { get; set; }
        /// <summary>是否将属性类型标记为可空。</summary>
        public bool IsNullable { get; set; }
        /// <summary>属性初始值表达式，如 <c>= ""</c> 或 <c>= "default"</c>。</summary>
        public string? DefaultValue { get; set; }
    }

    /// <summary>
    /// 生成成员的访问修饰符。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum AccessibilityEnum
    {
        /// <summary>不输出访问修饰符。</summary>
        NotApplicable = 0,
        /// <summary><c>private</c></summary>
        Private = 1,
        /// <summary><c>private protected</c></summary>
        ProtectedAndInternal = 2,
        /// <summary><c>protected</c></summary>
        Protected = 3,
        /// <summary><c>internal</c></summary>
        Internal = 4,
        /// <summary><c>protected internal</c></summary>
        ProtectedOrInternal = 5,
        /// <summary><c>public</c></summary>
        Public = 6
    }

    /// <summary>
    /// 生成文件的 <c>#nullable</c> 指令取值。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum NullableEnum
    {
        /// <summary>将可为空注释和警告上下文设置为“已禁用”。</summary>
        Disable,
        /// <summary>将可为空注释和警告上下文设置为“已启用”。</summary>
        Enable,
        /// <summary>将可为空注释和警告上下文还原为项目设置。</summary>
        Restore,
        /// <summary>将可为空注释上下文设置为“已禁用”。</summary>
        DisableAnnotations,
        /// <summary>将可为空注释上下文设置为“已启用”。</summary>
        EnableAnnotations,
        /// <summary>将可为空注释上下文还原为项目设置。</summary>
        RestoreAnnotations,
        /// <summary>将可为空警告上下文设置为“已禁用”。</summary>
        DisableWarnings,
        /// <summary>将可为空警告上下文设置为“已启用”。</summary>
        EnableWarnings,
        /// <summary>将可为空警告上下文还原为项目设置。</summary>
        RestoreWarnings
    }

    /// <summary>
    /// 生成类的代码样式。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum ClassStyleEnum
    {
        /// <summary>普通 POCO，属性为 <c>{ get; set; }</c>。</summary>
        Poco,
        /// <summary>Record 风格，带主构造函数，属性为 <c>{ get; }</c>。</summary>
        Record,
        /// <summary>INPC 风格，setter 内调用 <c>RaisePropertyChanged()</c>。</summary>
        Inpc
    }

    /// <summary>
    /// Inpc 模式的默认基类，提供属性存取与变更通知钩子。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public abstract class InpcPropertyBindableBaseAbstract
    {
        /// <summary>属性值字典。</summary>
        protected internal global::System.Collections.Generic.Dictionary<string, object> keyValuePairs = new global::System.Collections.Generic.Dictionary<string, object>();
        /// <summary>
        /// 获取属性值。
        /// </summary>
        /// <typeparam name="T">属性类型。</typeparam>
        /// <param name="defaultValue">不存在当前值时返回的默认值。</param>
        /// <param name="propertyName">属性名称。</param>
        /// <returns>已存储的值，或默认值。</returns>
        protected virtual T GetValue<T>(T defaultValue = default!, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName != null && keyValuePairs.TryGetValue(propertyName, out var value))
                return (T)value;
            return defaultValue;
        }
        /// <summary>
        /// 设置属性值。
        /// </summary>
        /// <typeparam name="T">属性类型。</typeparam>
        /// <param name="value">需要设置的属性值。</param>
        /// <param name="propertyName">属性名称。</param>
        protected virtual void SetValue<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName != null)
                keyValuePairs[propertyName] = value!;
        }
        /// <summary>
        /// 属性值变更时触发；子类可重写以实现 INotifyPropertyChanged。
        /// </summary>
        /// <param name="propertyName">发生变更的属性名。</param>
        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) { }
    }

    /// <summary>
    /// 按模式 Id 批量向类或属性附加特性字符串。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class CustomAttributeAttribute : Attribute
    {
        /// <summary>关联的单个模式 Id。</summary>
        public string? Id { get; set; }
        /// <summary>关联的多个模式 Id。</summary>
        public string[]? Ids { get; set; }
        /// <summary>要附加的特性字符串列表。</summary>
        public string[]? Attributes { get; set; }
    }
}
