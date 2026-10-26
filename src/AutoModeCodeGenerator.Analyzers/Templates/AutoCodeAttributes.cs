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
        /// <summary>关联的模式 Id，与 <see cref="AutoCodePropertyAttribute.Ids"/> 对应。</summary>
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
    /// 标准 CRUD 六模式请改用 <see cref="AutoCodeCrudModesAttribute"/>；
    /// 自定义 mode 或与预设并存覆盖时仍可使用本特性。
    /// </summary>
    [global::System.Obsolete("标准 CRUD 六模式请改用 AutoCodeCrudModes；自定义 mode / 与预设并存覆盖时仍可使用 AutoCodeClassModes。", error: false)]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class AutoCodeClassModesAttribute : AutoCodeClassBaseAttribute
    {
        /// <summary>基类类型名（字符串形式）。请优先使用 <see cref="InheritType"/>。</summary>
        [global::System.Obsolete("请改用 InheritType。", error: false)]
        public string? InheritStr { get; set; }
        /// <summary>基类类型（<see cref="Type"/> 形式）。</summary>
        public Type? InheritType { get; set; }
        /// <summary>实现的接口类型名列表（字符串形式）。请优先使用 <see cref="InterfaceTypes"/>。</summary>
        [global::System.Obsolete("请改用 InterfaceTypes。", error: false)]
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
        /// <summary>
        /// 为 <c>true</c> 且未指定 <see cref="InheritStr"/> / <see cref="InheritType"/> 时，
        /// 自动继承 <c>{源类型名}{InheritDtoSuffix}</c>（默认 <c>ProductDto</c>）。
        /// </summary>
        public bool InheritDto { get; set; }
        /// <summary>与 <see cref="InheritDto"/> 配合的 Dto 后缀，默认 <c>Dto</c>。</summary>
        public string InheritDtoSuffix { get; set; } = "Dto";
        /// <summary>
        /// 为 <c>true</c> 时，对 <see cref="InterfaceTypes"/> 中尚未投影的成员自动生成
        /// Entity + Dto 绑定，以及 Search（DateTime/数值等使用区间模板）。
        /// Detail 默认继承 Dto，不再单独投影，避免 CS0108。
        /// </summary>
        public bool InjectMarkerMembers { get; set; }
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
        /// <summary>覆盖属性类型（字符串形式）。请优先使用 <see cref="PropertyType"/>。</summary>
        [global::System.Obsolete("请改用 PropertyType。", error: false)]
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
        /// <summary>为 <c>true</c> 时生成 <c>init</c> 访问器（Poco/Record；Inpc 忽略）。</summary>
        public bool IsInit { get; set; }
        /// <summary>为 <c>true</c> 时生成 <c>required</c> 修饰符（Poco/Record；Inpc 忽略）。</summary>
        public bool IsRequired { get; set; }
        /// <summary>
        /// 用标志位指定模式，等价于填写 <see cref="Ids"/>。
        /// 可与 <see cref="Id"/> / <see cref="Ids"/> 并用（取并集）。
        /// </summary>
        public AutoCodeModeFlags ModeFlags { get; set; }
    }

    /// <summary>
    /// CRUD 模式标志（用于 <see cref="AutoCodePropertyAttribute.ModeFlags"/> 压缩常见 Id 组合）。
    /// </summary>
    [global::System.Flags]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum AutoCodeModeFlags
    {
        None = 0,
        Entity = 1,
        Dto = 2,
        Create = 4,
        Update = 8,
        Detail = 16,
        Search = 32,
        /// <summary>Entity + Dto + Search（最常见的可读字段）。</summary>
        EntityDtoSearch = Entity | Dto | Search,
        /// <summary>
        /// Entity + Dto + Detail。若 Create/Update/Detail 继承 Dto 且类型相同，优先用 <see cref="EntityDto"/>，
        /// 否则 Detail 会重复声明并可能 CS0108。
        /// </summary>
        EntityDtoDetail = Entity | Dto | Detail,
        /// <summary>Entity + Dto。</summary>
        EntityDto = Entity | Dto,
        /// <summary>Entity + Detail。</summary>
        EntityDetail = Entity | Detail,
        /// <summary>Entity + Dto + Detail + Search。</summary>
        EntityDtoDetailSearch = Entity | Dto | Detail | Search,
        /// <summary>全部六模式。</summary>
        AllCrud = Entity | Dto | Create | Update | Detail | Search,
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
        /// <summary>普通 POCO，属性为 <c>{ get; set; }</c>（可用 IsInit 改为 <c>init</c>）。</summary>
        Poco,
        /// <summary>生成真正的 <c>record class</c>，带构造函数，属性为 <c>{ get; init; }</c>。</summary>
        Record,
        /// <summary>INPC 风格，setter 内调用 <c>RaisePropertyChanged()</c>。</summary>
        Inpc
    }

    /// <summary>
    /// Inpc 模式的默认基类，实现 <see cref="global::System.ComponentModel.INotifyPropertyChanged"/>。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public abstract class InpcPropertyBindableBaseAbstract : global::System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>属性值字典。</summary>
        protected internal global::System.Collections.Generic.Dictionary<string, object> keyValuePairs = new global::System.Collections.Generic.Dictionary<string, object>();

        /// <inheritdoc />
        public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

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
        /// 设置属性值；值变化时触发 <see cref="PropertyChanged"/>。
        /// </summary>
        /// <typeparam name="T">属性类型。</typeparam>
        /// <param name="value">需要设置的属性值。</param>
        /// <param name="propertyName">属性名称。</param>
        protected virtual void SetValue<T>(T value, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                return;

            if (keyValuePairs.TryGetValue(propertyName, out var existing)
                && global::System.Collections.Generic.EqualityComparer<T>.Default.Equals((T)existing, value))
                return;

            keyValuePairs[propertyName] = value!;
            RaisePropertyChanged(propertyName);
        }
        /// <summary>
        /// 触发 <see cref="PropertyChanged"/>。
        /// </summary>
        /// <param name="propertyName">发生变更的属性名。</param>
        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 按模式 Id 批量向类或属性附加特性字符串。
    /// 请改用 <see cref="AutoCodeClassModesAttribute.Attributes"/> /
    /// <see cref="AutoCodePropertyAttribute.Attributes"/>。
    /// </summary>
    [global::System.Obsolete("请改用 AutoCodeClassModes / AutoCodeProperty 的 Attributes。", error: false)]
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

    /// <summary>
    /// CRUD 六模式 Id / 默认后缀常量。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public static class AutoCodeCrud
    {
        public const string Entity = "Entity";
        public const string Dto = "Dto";
        public const string Create = "CreateInput";
        public const string Update = "UpdateInput";
        public const string Detail = "DetailOutput";
        public const string Search = "SearchInput";

        public const string DefaultEntitySuffix = "Record";
        public const string DefaultDtoSuffix = "Dto";
        public const string DefaultCreateSuffix = "CreateInput";
        public const string DefaultUpdateSuffix = "UpdateInput";
        public const string DefaultDetailSuffix = "DetailOutput";
        public const string DefaultSearchSuffix = "SearchInput";
    }

    /// <summary>
    /// 从 <see cref="AutoCodeCrudModesAttribute"/> 展开时省略的模式。
    /// </summary>
    [global::System.Flags]
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum AutoCodeCrudOmit
    {
        None = 0,
        Entity = 1,
        Dto = 2,
        Create = 4,
        Update = 8,
        Detail = 16,
        Search = 32,
        EntityOnly = Dto | Create | Update | Detail | Search,
        NoCreate = Create,
        NoUpdate = Update,
    }

    /// <summary>
    /// 一次声明展开为 Entity/Dto/Create/Update/Detail/Search 六种模式。
    /// 与手写 <see cref="AutoCodeClassModesAttribute"/> 并存时：相同 Id 以手写为准。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class AutoCodeCrudModesAttribute : Attribute
    {
        public string EntitySuffix { get; set; } = AutoCodeCrud.DefaultEntitySuffix;
        public string DtoSuffix { get; set; } = AutoCodeCrud.DefaultDtoSuffix;
        public string CreateSuffix { get; set; } = AutoCodeCrud.DefaultCreateSuffix;
        public string UpdateSuffix { get; set; } = AutoCodeCrud.DefaultUpdateSuffix;
        public string DetailSuffix { get; set; } = AutoCodeCrud.DefaultDetailSuffix;
        public string SearchSuffix { get; set; } = AutoCodeCrud.DefaultSearchSuffix;
        public string? Namespace { get; set; }
        public string? NamespacePrefix { get; set; }
        public string? NamespaceSuffix { get; set; }
        /// <summary>Entity 实现的接口类型名（字符串）。请优先使用 <see cref="EntityInterfaceTypes"/>。</summary>
        [global::System.Obsolete("请改用 EntityInterfaceTypes。", error: false)]
        public string[]? EntityInterfaceStrs { get; set; }
        public Type[]? EntityInterfaceTypes { get; set; }
        public string[]? EntityAttributes { get; set; }
        public AutoCodeCrudOmit OmitModes { get; set; }
        public bool SearchToNullable { get; set; } = true;
        public bool CreateUpdateDetailInheritDto { get; set; } = true;
        public string CreateSummaryPrefix { get; set; } = "创建";
        public string UpdateSummaryPrefix { get; set; } = "更新";
        public string DetailSummaryPrefix { get; set; } = "详情";
        public string SearchSummaryPrefix { get; set; } = "搜索";
        /// <summary>
        /// 为 <c>true</c> 时，对 <see cref="EntityInterfaceTypes"/> 中尚未投影的成员自动生成
        /// Entity + Dto 与 Search 绑定（DateTime/数值等进区间类型；Detail 默认继承 Dto 不再单独投影）。
        /// 区间类型见 <see cref="SearchRangeType"/> / <see cref="SearchRangeTemplate"/> / MSBuild <c>AutoModeSearchRangeTemplate</c>。
        /// </summary>
        public bool InjectMarkerMembers { get; set; }
        /// <summary>
        /// Search 区间开泛型类型，如 <c>typeof(SearchRangeForStructValue&lt;&gt;)</c>。
        /// 与 <see cref="SearchRangeTemplate"/> 二选一即可；二者皆设时<strong>字符串优先</strong>；
        /// 皆空时用 MSBuild <c>AutoModeSearchRangeTemplate</c>（默认 <c>SearchRange&lt;{0}&gt;</c>）。
        /// </summary>
        public Type? SearchRangeType { get; set; }
        /// <summary>
        /// Search 区间类型模板，<c>{0}</c> 为元素类型。默认 <c>SearchRange&lt;{0}&gt;</c>。
        /// 优先于 <see cref="SearchRangeType"/> 与 MSBuild（非空时）。
        /// </summary>
        public string? SearchRangeTemplate { get; set; }
    }

    /// <summary>
    /// 将外键概念展开为 Id / Detail / Search / Navigate 等多条属性投影。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutoCodeRelationAttribute : Attribute
    {
        public string? TargetName { get; set; }
        public bool IncludeNavigate { get; set; } = true;
        public string NavigateAttribute { get; set; } =
            "global::SqlSugar.Navigate(global::SqlSugar.NavigateType.ManyToOne, nameof({0}Id))";
        public string IdSuffix { get; set; } = "Id";
    }

    /// <summary>
    /// 字段形态变换种类（文件 / 区间 / 枚举双轨 / CSV↔数组）。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum AutoCodeMorphKind
    {
        /// <summary>Entity 为标量；Create/Update 与 Detail 使用文件 DTO 类型；Search 省略。</summary>
        File = 0,
        /// <summary>仅 Search 使用区间类型（默认 <c>SearchRange&lt;T&gt;</c>）；其它模式需另标 <see cref="AutoCodePropertyAttribute"/>。</summary>
        SearchRange = 1,
        /// <summary>默认各 mode 仍为枚举类型，仅 Entity/Api 特性宏不同；设 EntityType=<c>string</c> 时 Entity 存字符串列。</summary>
        StringEnum = 2,
        /// <summary>Entity 存 CSV 字符串；Create/Update/Detail 使用 <c>string[]</c>；Search 省略。</summary>
        StringArray = 3,
    }

    /// <summary>
    /// 按 mode 展开不同属性类型与特性宏（可覆盖默认类型与 Attribute 字符串）。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutoCodeMorphAttribute : Attribute
    {
        public AutoCodeMorphKind Kind { get; set; }

        /// <summary>Entity 模式类型覆盖（File/StringEnum/StringArray 默认多为 <c>string</c>）。</summary>
        public string? EntityType { get; set; }
        /// <summary>Create/Update 类型覆盖（File 默认 <c>FileCreateInput</c>）。</summary>
        public string? CreateUpdateType { get; set; }
        /// <summary>Detail 类型覆盖（File 默认 <c>FileOutput</c>）。</summary>
        public string? DetailType { get; set; }
        /// <summary>Search 类型覆盖（SearchRange 默认模板见 <see cref="RangeType"/> / <see cref="RangeTypeTemplate"/>）。</summary>
        public string? SearchType { get; set; }
        /// <summary>StringEnum：API 模式使用的枚举类型名。</summary>
        public string? EnumType { get; set; }
        /// <summary>
        /// SearchRange：区间开泛型，如 <c>typeof(SearchRangeForStructValue&lt;&gt;)</c>。
        /// 与 <see cref="RangeTypeTemplate"/> 二选一；字符串优先；皆空则用类级/MSBuild 默认。
        /// </summary>
        public Type? RangeType { get; set; }
        /// <summary>SearchRange：区间类型模板，<c>{0}</c> 为源属性类型。优先于 <see cref="RangeType"/>。</summary>
        public string? RangeTypeTemplate { get; set; }

        /// <summary>附加到 Entity 模式的特性字符串（可多条）。</summary>
        public string[]? EntityAttributes { get; set; }
        /// <summary>附加到 API 模式（Dto/Create/Update/Detail/Search）的特性字符串。</summary>
        public string[]? ApiAttributes { get; set; }
    }

    /// <summary>
    /// 常用特性宏字符串（ORM 无关字面量；SqlSugar 示例见 Conventions 文档）。可直接赋给 <c>Attributes</c> / Morph。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public static class AutoCodeAttrMacros
    {
        public const string Serializable = "global::System.Serializable";
        public const string SugarPrimaryKey =
            "global::SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)";
        public const string SugarPrimaryKeyNoIdentity =
            "global::SqlSugar.SugarColumn(IsPrimaryKey = true)";
        public const string SugarColumnIgnore =
            "global::SqlSugar.SugarColumn(IsIgnore = true)";
        public const string SugarBigString =
            "global::SqlSugar.SugarColumn(ColumnDataType = global::SqlSugar.StaticConfig.CodeFirst_BigString)";
        public const string StringLength64 =
            "global::System.ComponentModel.DataAnnotations.StringLength(64)";
        public const string StringLength128 =
            "global::System.ComponentModel.DataAnnotations.StringLength(128)";
        public const string StringLength1024 =
            "global::System.ComponentModel.DataAnnotations.StringLength(1024)";
        public const string StringLengthMax =
            "global::System.ComponentModel.DataAnnotations.StringLength(0x7fffffff)";
        public const string JsonIgnore =
            "global::System.Text.Json.Serialization.JsonIgnore";
        public const string OpenApiStringEnum =
            "global::System.Text.Json.Serialization.JsonConverter(typeof(global::System.Text.Json.Serialization.JsonStringEnumConverter))";

        /// <summary>
        /// SqlSugar 日期列：将 <c>{0}</c> 替换为项目内 DateTime 转换器全名，例如
        /// <c>string.Format(SugarColumnDateTimeDbType, "global::MyApp.DateTimeConverter")</c>。
        /// </summary>
        public const string SugarColumnDateTimeDbType =
            "global::SqlSugar.SugarColumn(SqlParameterDbType = typeof({0}))";
    }

    /// <summary>列排序方向（用于 <see cref="AutoCodeIndexAttribute"/>）。</summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    public enum AutoCodeOrderBy
    {
        Asc = 0,
        Desc = 1,
    }

    /// <summary>
    /// 属性级列约束短语法：展开为 Entity 模式上的 StringLength / SugarColumn 等特性。
    /// 可与 <see cref="AutoCodePropertyAttribute"/> 并用（特性合并到 Entity 投影）。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class AutoCodeColumnAttribute : Attribute
    {
        /// <summary>字符串最大长度；生成 <c>StringLength(n)</c>。与 <see cref="Max"/> / <see cref="BigString"/> 互斥优先：BigString &gt; Max &gt; Length。</summary>
        public int Length { get; set; }
        /// <summary>为 true 时使用极大 StringLength（等价项目内 StringMax）。</summary>
        public bool Max { get; set; }
        /// <summary>为 true 时使用 SqlSugar BigString 列类型。</summary>
        public bool BigString { get; set; }
        /// <summary>可选列名，生成 <c>SugarColumn(ColumnName = "...")</c>。</summary>
        public string? Name { get; set; }
        /// <summary>附加到 Entity 的额外特性字符串。</summary>
        public string[]? ExtraAttributes { get; set; }
    }

    /// <summary>
    /// 类级索引声明，展开为 Entity 模式上的 <c>SugarIndex(...)</c> 特性字符串。
    /// </summary>
    [global::System.Runtime.CompilerServices.CompilerGenerated]
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class AutoCodeIndexAttribute : Attribute
    {
        public AutoCodeIndexAttribute() { }

        /// <param name="name">索引名。</param>
        /// <param name="columns">列名列表（默认 Asc）。</param>
        public AutoCodeIndexAttribute(string name, params string[] columns)
        {
            Name = name;
            Columns = columns;
        }

        /// <summary>索引名。</summary>
        public string? Name { get; set; }
        /// <summary>是否唯一索引。</summary>
        public bool Unique { get; set; }
        /// <summary>列名（与 <see cref="Orders"/> 按下标对齐；缺省 Order 为 Asc）。</summary>
        public string[]? Columns { get; set; }
        /// <summary>与 Columns 对齐的排序（<see cref="AutoCodeOrderBy"/>）。</summary>
        public AutoCodeOrderBy[]? Orders { get; set; }
        /// <summary>
        /// 紧凑写法，例如 <c>"CreateTime:Desc"</c> 或 <c>"ProductId:Asc,DeviceName:Asc,CreateTime:Desc"</c>。
        /// 若同时指定 <see cref="Columns"/>，以 Columns 为准。
        /// </summary>
        public string? Spec { get; set; }
    }
}
