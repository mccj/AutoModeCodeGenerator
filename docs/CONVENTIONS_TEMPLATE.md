# Conventions 模板（可复制）

目标：减少各项目复制 `AutoGeneratorConst` / 审计 mixin。主包 **不依赖** 任何 ORM；SqlSugar 字符串仅作示例。

另见可选源码包：[`src/AutoModeCodeGenerator.Conventions.SqlSugar/`](../src/AutoModeCodeGenerator.Conventions.SqlSugar/)。

> **2.5+** 推荐：列用 `[AutoCodeColumn]`、索引用 `[AutoCodeIndex]`、属性 Id 用 `ModeFlags`；审计可用 **`InjectMarkerMembers = true`** 或下方 CreateTime mixin。只挂 `ICreateTimeEntity` 却不投影 → **Mccj011**。

## 1. Id / Suffix 常量

2.2+ 推荐直接用生成器注入的 `AutoCodeCrud`。若需项目别名：

```csharp
using AutoCodeGenerator;

namespace MyApp;

public static class AutoGeneratorConst
{
    public const string EntityId = AutoCodeCrud.Entity;
    public const string EntitySuffix = AutoCodeCrud.DefaultEntitySuffix; // 内控可改为 "Entity"
    public const string DtoId = AutoCodeCrud.Dto;
    public const string DtoSuffix = AutoCodeCrud.DefaultDtoSuffix;
    public const string CreateId = AutoCodeCrud.Create;
    public const string UpdateId = AutoCodeCrud.Update;
    public const string DetailId = AutoCodeCrud.Detail;
    public const string SearchId = AutoCodeCrud.Search;
}
```

常用 Id 组合可用 **`ModeFlags`**（无需自建数组常量）：

```csharp
[AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
string? Title { get; set; }

[AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoDetail)]
[AutoCodeColumn(BigString = true)]
string? Content { get; set; }
```

| ModeFlags | 等价 Ids |
|-----------|----------|
| `EntityDtoSearch` | Entity, Dto, Search |
| `EntityDtoDetail` | Entity, Dto, Detail |
| `EntityDtoDetailSearch` | Entity, Dto, Detail, Search |
| `EntityDto` / `EntityDetail` / `AllCrud` | 见枚举 |

## 2. 主键 / 审计 mixin

### 2.1 主键

```csharp
using AutoCodeGenerator;

namespace MyApp;

public interface IHasLongId
{
    [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
    [AutoCodeProperty(Ids = [AutoCodeCrud.Update, AutoCodeCrud.Detail])]
    long Id { get; set; }
}
```

### 2.2 CreateTime（与 `ICreateTimeEntity<T>` 对齐）

运行时标记接口 **不会** 自动生成属性。必须同时继承带 `AutoCodeProperty` 的 mixin：

```csharp
/// <summary>属性投影用；名称必须为 CreateTime。</summary>
public interface IHasCreateTime
{
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Detail])]
    [AutoCodeProperty(Ids = [AutoCodeCrud.Search], PropertyTypeStr = "SearchRangeForStructValue<DateTime>")]
    // 或: [AutoCodeMorph(Kind = AutoCodeMorphKind.SearchRange, RangeTypeTemplate = "SearchRangeForStructValue<{0}>")]
    DateTime? CreateTime { get; set; }
}

// 运行时约束（SqlSugar / 统计查询等）
public interface ICreateTimeEntity<T> : IEntity<T>
{
    DateTime? CreateTime { get; set; }
}

[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(ICreateTimeEntity<long>)])]
[AutoCodeIndex("IX_Boot_CreateTime", Spec = "CreateTime:Desc")]
public interface DeviceBootLogs : IHasLongId, IHasCreateTime
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    string? DeviceModel { get; set; }
}

// 或 2.5 自动注入（无需 IHasCreateTime）：
[AutoCodeCrudModes(
    EntityInterfaceTypes = [typeof(ICreateTimeEntity<long>)],
    InjectMarkerMembers = true,
    SearchRangeType = typeof(SearchRangeForStructValue<>))]
public interface DeviceBootLogsAuto : IHasLongId
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    string? DeviceModel { get; set; }
}
```

项目级默认区间类型：

```xml
<AutoModeSearchRangeTemplate>SearchRangeForStructValue&lt;{0}&gt;</AutoModeSearchRangeTemplate>
```

日期列转换器：

```csharp
Attributes = [string.Format(AutoCodeAttrMacros.SugarColumnDateTimeDbType, "global::AutoGeneratorModels.DateTimeConverter")]
// 或 SqlSugarAttrTemplates.DateTimeConverter("global::AutoGeneratorModels.DateTimeConverter")
```

### 2.3 CreatedTime（Admin.NET 风格，勿与 CreateTime 混用）

```csharp
public interface IHasCreatedTimeAudit
{
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Detail])]
    [AutoCodeProperty(Ids = [AutoCodeCrud.Search], PropertyTypeStr = "SearchRangeForStructValue<DateTime>")]
    DateTime? CreatedTime { get; set; }

    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Detail])]
    [AutoCodeProperty(Ids = [AutoCodeCrud.Search], PropertyTypeStr = "SearchRangeForStructValue<DateTime>")]
    DateTime? UpdatedTime { get; set; }
}
```

## 3. 列与索引（2.4）

```csharp
using AutoCodeGenerator;
using AutoModeCodeGenerator.Conventions.SqlSugar; // 可选

[AutoCodeCrudModes(EntityAttributes = [SqlSugarAttrTemplates.Table("ThingModelInboundRaw")])]
[AutoCodeIndex("IX_InboundRaw_CreateTime", Spec = "CreateTime:Desc")]
[AutoCodeIndex(Name = "IX_InboundRaw_Device_CreateTime", Spec = "DeviceName:Asc,CreateTime:Desc")]
[AutoCodeIndex(Name = "UK_PropertyCurrent_DeviceAttr", Unique = true,
    Columns = ["ProductId", "DeviceName", "ThingModelName", "PropertyKey"])]
public interface ThingModelInboundRaw : IHasLongId, IHasCreateTime
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    [AutoCodeColumn(Length = 64)]
    string? ProductId { get; set; }

    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoDetail)]
    [AutoCodeColumn(BigString = true)]
    string? PlatformWire { get; set; }
}
```

等价手写特性可用 `SqlSugarAttrTemplates.StringLength64` / `Index(...)` / `AutoCodeAttrMacros.*`。

## 4. Entity-only 关联表

```csharp
[AutoCodeCrudModes(OmitModes = AutoCodeCrudOmit.EntityOnly)]
public interface ProductConfigFirmwareVersionMapping : IHasLongId
{
    [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
    long ProductConfigId { get; set; }

    [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
    long FirmwareVersionId { get; set; }
}
```

## 5. 手写 ClassModes 时的 InheritDto

```csharp
[AutoCodeClassModes(Id = AutoCodeCrud.Create, Suffix = AutoCodeCrud.DefaultCreateSuffix, InheritDto = true)]
[AutoCodeClassModes(Id = AutoCodeCrud.Update, Suffix = AutoCodeCrud.DefaultUpdateSuffix, InheritDto = true)]
[AutoCodeClassModes(Id = AutoCodeCrud.Detail, Suffix = AutoCodeCrud.DefaultDetailSuffix, InheritDto = true)]
```

新代码仍优先 `[AutoCodeCrudModes]`（手写六行会触发 **Mccj012** Info）。

## 6. Morph 与特性宏

主包 `AutoCodeAttrMacros`（2.4 含长度档位）。Morph 示例：

```csharp
[AutoCodeMorph(
    Kind = AutoCodeMorphKind.StringEnum,
    EnumType = nameof(PayStatus),
    EntityAttributes = [AutoCodeAttrMacros.SugarColumnIgnore],
    ApiAttributes = [AutoCodeAttrMacros.OpenApiStringEnum])]
PayStatus Status { get; set; }

[AutoCodeMorph(Kind = AutoCodeMorphKind.File)]
string Image { get; set; }

[AutoCodeMorph(Kind = AutoCodeMorphKind.StringArray, EntityAttributes = [AutoCodeAttrMacros.StringLengthMax])]
string[]? ThingModelTypes { get; set; }
```
