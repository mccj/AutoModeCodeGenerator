# AutoModeCodeGenerator.Analyzers

基于 Roslyn 的 C# **增量源生成器**：用接口规格在编译期生成 **SqlSugar / Admin.NET 风格** 的 Entity / Dto / Create / Update / Detail / Search 等类型（数据形状；不生成 Service / Controller）。

**当前版本：1.0.1.0**

**使用目的：** 一份规格 → 多 CRUD 产物；按 mode 投影字段；对接现有 ORM + AppService，消灭六模式/外键样板。亦支持 `.entity.json` 声明式生成。

**双主入口：**（A）规格接口 + `[AutoCodeCrudModes]` / Relation / Morph → 六产物；（B）`.entity.json` → 声明式类型。共享 Emit。详见 [AI 指南 §0](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/AI_CONSUMER_GUIDE.md#0-30-%E7%A7%92%E5%AE%9A%E4%BD%8D)·[ARCHITECTURE](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/ARCHITECTURE.md)。

**CRUD 推荐：** `[AutoCodeCrudModes]` + Relation / Morph / Column / Index；审计可用 `InjectMarkerMembers`。`ClassStyle` 默认 **Poco**；Record / Inpc 按需。

**1.0.1：** `AutoCodeClassModes` / `*Str` / `CustomAttribute` 软弃用（`[Obsolete]`，行为保留）。AppVeyor 发包格式 `1.0.1.{build}`。

| 文档 | 说明 |
|------|------|
| [CHANGELOG](https://github.com/mccj/AutoModeCodeGenerator/blob/main/CHANGELOG.md) | 版本变更 |
| [升级指南](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/UPGRADE.md) | 1.0.0.x → 1.0.1.x（含 Obsolete 表） |
| [AI/Agent 指南](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/AI_CONSUMER_GUIDE.md) | **目的/架构/优势**、决策树、禁忌 |
| [Conventions 模板](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/CONVENTIONS_TEMPLATE.md) | mixin / 列索引 |
| [路线图](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/ROADMAP.md) | 1.0.1 之后建议与非目标 |
| [Analyzers DX 改进](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/ANALYZER_DX_IMPROVEMENTS.md) | DeviceServer 试点驱动的诊断/CodeFix 细则 |
| NuGet / 源码 | <https://www.nuget.org/packages/AutoModeCodeGenerator.Analyzers> · <https://github.com/mccj/AutoModeCodeGenerator> |

## 目录

- [安装](#安装)
- [1.0.1 破坏性变更（相对 1.0.0.17）](#101-破坏性变更相对-10017)
- [推荐：CRUD 预设](#推荐crud-预设)
- [Morph 字段变换](#morph-字段变换)
- [列 / 索引 / ModeFlags](#列--索引--modeflags)
- [注入 / SearchRange / CodeFix](#注入--searchrange--codefix)
- [手写六模式（Obsolete）](#手写六模式obsolete)
- [快速开始（最小示例）](#快速开始最小示例)
- [核心概念](#核心概念)
- [特性参考](#特性参考)
- [诊断](#诊断)
- [从 entity.json 生成](#从-entityjson-生成)
- [验证与测试](#验证与测试)

## 安装

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.1.0" PrivateAssets="all" />
```

`PrivateAssets="all"` 确保生成器仅参与编译，不传递到依赖项目的运行时引用。

### 可选 MSBuild 开关

包会自动导入 `build/AutoModeCodeGenerator.Analyzers.props`。可在消费项目的 `PropertyGroup` 中覆盖：

```xml
<PropertyGroup>
  <!-- 双主入口 B：处理 AdditionalFiles 中的 .entity.json（默认 true） -->
  <AutoModeGenerateEntityFiles>true</AutoModeGenerateEntityFiles>
  <AutoModeEntityFileSuffix>.entity.json</AutoModeEntityFileSuffix>
  <!-- Morph.SearchRange / InjectMarkerMembers 默认区间类型（{0}=元素类型） -->
  <AutoModeSearchRangeTemplate>SearchRangeForStructValue&lt;{0}&gt;</AutoModeSearchRangeTemplate>
</PropertyGroup>
```

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `AutoModeGenerateEntityFiles` | `true` | 是否处理 `AdditionalFiles` 中的 entity 文件（入口 B） |
| `AutoModeEntityFileSuffix` | `.entity.json` | 匹配的附加文件后缀（大小写不敏感） |
| `AutoModeSearchRangeTemplate` | `SearchRange<{0}>` | Search 区间类型模板；可被 `AutoCodeCrudModes.SearchRangeTemplate` 覆盖 |

### 可选：将生成代码输出到磁盘

便于在 IDE 中查看生成结果：

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
```

## 1.0.1 破坏性变更（相对 1.0.0.17）

从 `1.0.0.x` 升级到 `1.0.1.x` 时请注意（Record / Inpc 语义）：

| 变更 | 影响 |
|------|------|
| `ClassStyle.Record` 生成真正的 `record class` | 生成类型具备值相等与 `with` 语义；属性为 `{ get; init; }` |
| Inpc 基类实现 `INotifyPropertyChanged` | `SetValue` 会触发 `PropertyChanged`；生成代码不再额外调用 `RaisePropertyChanged()` |
| 内部命名空间重命名 | 仅影响仓库测试/内部代码，公共命名空间仍为 `AutoCodeGenerator` |

完整说明见仓库 [CHANGELOG.md](https://github.com/mccj/AutoModeCodeGenerator/blob/main/CHANGELOG.md)。

## 推荐：CRUD 预设

默认 **Poco**。用一行预设替代六行 `AutoCodeClassModes`；属性 Id 使用内置 `AutoCodeCrud` 常量。

```csharp
using AutoCodeGenerator;

[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(IEntityMarker)])]
public interface Product : IHasLongId
{
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search])]
    string Name { get; set; }

    [AutoCodeRelation(TargetName = nameof(Category))]
    long Category { get; set; }
}
```

### 模式约定（预设默认值）

| Mode `Id`（`AutoCodeCrud.*`） | 默认 `Suffix` | 要点 |
|-------------------------------|---------------|------|
| `Entity` | `Record` | 可设 `EntityInterfaceTypes` / `EntityAttributes` |
| `Dto` | `Dto` | Create/Update/Detail 默认 `InheritStr` 指向 `*Dto` |
| `Create` / `Update` / `Detail` | `*Input` / `*Output` | 可用 `*Suffix` / `*SummaryPrefix` 覆盖 |
| `Search` | `SearchInput` | 默认 `SearchToNullable = true` |

常用选项：`OmitModes`（如 `AutoCodeCrudOmit.EntityOnly`）、`EntitySuffix`（内控等可改为 `"Entity"`）、`CreateUpdateDetailInheritDto`。

**合并规则：** 同一 `Id` 的手写 `AutoCodeClassModes` **覆盖**预设展开结果。

### AutoCodeRelation

在外键属性上声明，默认展开：

1. `{Name}Id` → Entity / Dto / Search  
2. Detail 类型投影（可空）  
3. Search 类型投影  
4. Entity 上的 Navigate 属性（默认 SqlSugar 字符串模板；`IncludeNavigate = false` 可关；`NavigateAttribute` 可换 EF 等）

### 共享 mixin

将 `Id`、审计字段放在基接口上，仅标 `AutoCodeProperty`（Ids 用 `AutoCodeCrud.*`）。

Samples：[`CrudCookbook/CrudProduct.cs`](https://github.com/mccj/AutoModeCodeGenerator/blob/main/src/AutoModeCodeGenerator.Samples/CrudCookbook/CrudProduct.cs)。升级步骤见 [UPGRADE.md](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/UPGRADE.md)。

## Morph 字段变换

```csharp
[AutoCodeMorph(Kind = AutoCodeMorphKind.File, CreateUpdateType = "UploadFileInput", DetailType = "FileOutput")]
string Receipt { get; set; }

[AutoCodeMorph(Kind = AutoCodeMorphKind.StringEnum, EnumType = nameof(PayStatus),
    EntityAttributes = [AutoCodeAttrMacros.SugarColumnIgnore],
    ApiAttributes = [AutoCodeAttrMacros.JsonIgnore])]
PayStatus Status { get; set; }

[AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto])]
[AutoCodeMorph(Kind = AutoCodeMorphKind.SearchRange)]
DateTime Created { get; set; }

[AutoCodeMorph(Kind = AutoCodeMorphKind.StringArray)]
string Tags { get; set; }
```

| Kind | Entity | Dto（Create/Update 继承） | Detail | Search |
|------|--------|---------------------------|--------|--------|
| `File` | `string`（可覆盖） | `FileCreateInput` | `FileOutput`（异于 Dto 时投影） | — |
| `StringEnum` | `string` | 枚举 | 继承 Dto | 枚举 |
| `SearchRange` | （另标 Property） | — | — | `SearchRange<T>` |
| `StringArray` | `string` | `string[]` | 默认同 Dto | — |

类型与 `EntityAttributes` / `ApiAttributes` 均可项目级覆盖。约定模板：[CONVENTIONS_TEMPLATE.md](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/CONVENTIONS_TEMPLATE.md)。

## 列 / 索引 / ModeFlags

```csharp
[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(ICreateTimeEntity<long>)])]
[AutoCodeIndex("IX_Raw_CreateTime", Spec = "CreateTime:Desc")]
[AutoCodeIndex(Name = "UK_Device", Unique = true, Columns = ["DeviceName"])]
public interface InboundRaw : IHasLongId, IHasCreateTime
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    [AutoCodeColumn(Length = 64)]
    string? DeviceName { get; set; }

    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoDetail)]
    [AutoCodeColumn(BigString = true)]
    string? PlatformWire { get; set; }
}
```

| API | 说明 |
|-----|------|
| `ModeFlags` | 压缩 `Ids`（如 `EntityDtoSearch`）；可与 `Ids` 并集 |
| `AutoCodeColumn` | Entity 上 StringLength / BigString / ColumnName |
| `AutoCodeIndex` | Entity 类 `SugarIndex`；列须已投影否则 **Mccj013** |
| `InheritDto` | 手写 ClassModes 时自动 `InheritStr = {Name}Dto` |

**注意：** `EntityInterfaceTypes` 不会自动投影属性。使用 `ICreateTimeEntity` 时必须有带 `AutoCodeProperty` 的 `CreateTime` mixin，否则 **Mccj011**（并最终 CS0535）。模板见 [CONVENTIONS_TEMPLATE.md](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/CONVENTIONS_TEMPLATE.md)。

手写标准六模式会收到 **Mccj012**（Info）提示改用 `AutoCodeCrudModes`。

## 注入 / SearchRange / CodeFix

```csharp
[AutoCodeCrudModes(
    EntityInterfaceTypes = [typeof(ICreateTimeEntity<long>)],
    InjectMarkerMembers = true,
    SearchRangeType = typeof(SearchRangeForStructValue<>))] // 亦可用 SearchRangeTemplate 字符串
public interface BootLog : IHasLongId { /* ... */ }
```

| 能力 | 说明 |
|------|------|
| `InjectMarkerMembers` | 为 `EntityInterfaceTypes` / `InterfaceTypes` 中尚未投影的成员补 Entity/Dto（DateTime 等进 Search 区间） |
| `SearchRangeType` | `typeof(开泛型<>)`，展开为 `…<{0}>`（1.0.1） |
| `SearchRangeTemplate` | 字符串模板；与 Type **兼容**，非空时优先于 Type；再空则用 MSBuild `AutoModeSearchRangeTemplate` |
| Morph `RangeType` / `RangeTypeTemplate` | 属性级同样支持 Type 或字符串（字符串优先） |
| Detail 去重 | Create/Update/Detail 继承 Dto 且同名同类型时不重复生成属性（避免 CS0108） |
| Mccj012 Code Fix | IDE：「改用 AutoCodeCrudModes」一键替换标准六行 ClassModes |

日期列宏：`AutoCodeAttrMacros.SugarColumnDateTimeDbType`（`string.Format(..., "global::Your.DateTimeConverter")`）。

## 手写六模式（Obsolete）

1.0.1 起 `AutoCodeClassModes` / `*Str` 带 `[Obsolete]`（`error: false`），**行为保留**。标准六模式请改用 `AutoCodeCrudModes`（或 Mccj012 Code Fix）。自定义 mode / 与预设同 Id 覆盖时仍可用 ClassModes。

```csharp
using C = MyApp.AutoGeneratorConst;

[AutoCodeClassModes(Id = C.DtoId, Suffix = C.DtoSuffix)]
[AutoCodeClassModes(Id = C.EntityId, Suffix = C.EntitySuffix, InterfaceTypes = [typeof(IEntity<long>)])]
[AutoCodeClassModes(Id = C.CreateId, InheritStr = nameof(Product) + C.DtoSuffix, Suffix = C.CreateSuffix, SummaryPrefix = "创建")]
[AutoCodeClassModes(Id = C.UpdateId, InheritStr = nameof(Product) + C.DtoSuffix, Suffix = C.UpdateSuffix, SummaryPrefix = "更新")]
[AutoCodeClassModes(Id = C.DetailId, InheritStr = nameof(Product) + C.DtoSuffix, Suffix = C.DetailSuffix, SummaryPrefix = "详情")]
[AutoCodeClassModes(Id = C.SearchId, ToNullable = true, Suffix = C.SearchSuffix, SummaryPrefix = "搜索")]
public interface Product : IHasId<long>
{
    [AutoCodeProperty(Ids = [C.EntityId, C.DtoId, C.SearchId])]
    string Name { get; set; }
}
```

外键四段式（等价于 `AutoCodeRelation`）：

```csharp
[AutoCodeProperty(Ids = [C.EntityId, C.DtoId, C.SearchId], Suffix = "Id")]
[AutoCodeProperty(Ids = [C.DetailId], PropertyTypeStr = nameof(Category) + C.DetailSuffix, IsNullable = true)]
[AutoCodeProperty(Ids = [C.SearchId], PropertyTypeStr = nameof(Category) + C.SearchSuffix)]
[AutoCodeProperty(
    Ids = [C.EntityId],
    PropertyTypeStr = nameof(Category) + C.EntitySuffix,
    IsNullable = true,
    Attributes = [$"global::SqlSugar.Navigate(global::SqlSugar.NavigateType.ManyToOne, nameof({nameof(Category)}Id))"])]
public long Category { get; set; }
```

---


## 快速开始（最小示例）


1. 在**接口或类**上添加 `[AutoCodeClassModes]`，指定唯一 `Id` 及生成选项。
2. 在需要导出的**属性**上添加 `[AutoCodeProperty]`，通过 `Id` / `Ids` 关联到对应的类模式。
3. 编译后，生成器输出 `{命名空间}.{类型名}.g.cs`。

```csharp
#nullable enable

[AutoCodeGenerator.AutoCodeNullable(Type = AutoCodeGenerator.NullableEnum.Enable)]
[AutoCodeGenerator.AutoCodeClassModes(
    Id = "dto",
    Namespace = "MyApp.Models",
    Suffix = "Dto",
    ClassStyle = AutoCodeGenerator.ClassStyleEnum.Poco)]
public interface IUser
{
    [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
    string Name { get; set; }

    [AutoCodeGenerator.AutoCodeProperty(Id = "dto", IsNullable = true)]
    int Age { get; set; }
}
```

编译后生成类似：

```csharp
namespace MyApp.Models
{
    public partial class IUserDto
    {
        public string Name { get; set; } = null!;
        public int? Age { get; set; }
    }
}
```

### `init` / `required` 示例

```csharp
[AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "MyApp.Models")]
public interface IProduct
{
    [AutoCodeGenerator.AutoCodeProperty(Id = "dto", IsInit = true, IsRequired = true)]
    string Name { get; set; }
}
```

生成属性类似：`public required string Name { get; init; }`（仅 **Poco / Record**；**Inpc** 忽略这两项）。

完整示例见仓库 [TestModel.interface.cs](https://github.com/mccj/AutoModeCodeGenerator/blob/main/src/AutoModeCodeGenerator.Samples/TestModel.interface.cs)。

## 核心概念

### Id 关联机制

- `AutoCodeClassModes(Id = "a1", ...)` 定义一种生成模式。
- `AutoCodeProperty(Id = "a1")` 或 `Ids = new[] { "a1", "a2" }` 将属性绑定到一种或多种模式。
- 同一接口可配置多个 `AutoCodeClassModes`，一次定义生成多个不同命名空间/样式的类。
- 缺少或空白的 `Id` 会报告 Warning `Mccj004` 并跳过该模式；属性引用不存在的 Id 报告 `Mccj005`。

### 类样式（ClassStyle）

CRUD 与多数场景保持默认 **Poco**。`Record` / `Inpc` 按需选用（特性与 entity.json 均可）。

| 样式 | 生成形态 | 属性访问器 |
|------|----------|------------|
| `Poco`（默认） | `partial class` | `{ get; set; }`；`IsInit=true` 时为 `{ get; init; }` |
| `Record` | `partial record class` | `{ get; init; }`，并为每个属性生成构造函数参数 |
| `Inpc` | `partial class`，默认继承 `InpcPropertyBindableBaseAbstract` | getter/`SetValue`；忽略 `IsInit` / `IsRequired` |

**Inpc** 可显式指定基类；省略时自动使用内置基类：

```csharp
[AutoCodeClassModes(
    Id = "vm",
    ClassStyle = AutoCodeGenerator.ClassStyleEnum.Inpc,
    InheritType = typeof(AutoCodeGenerator.InpcPropertyBindableBaseAbstract))]
```

内置基类实现 `INotifyPropertyChanged`：`SetValue` 在值变化时触发 `PropertyChanged`。也可提供自己的基类（需提供兼容的 `GetValue` / `SetValue` / `RaisePropertyChanged` 成员）。

**Record** 构造函数参数名为属性名首字母小写（单字符属性加 `@`，如 `X` → `@X`）。

### 继承与接口成员

- 源类型继承的其他接口成员上的 `AutoCodeProperty` 也会参与生成。
- `InheritAttribute = true` 时，源属性上的非生成器 Attribute（如 `[DisplayName]`）会复制到生成类。

### 生成文件命名

| 来源 | 输出文件 |
|------|----------|
| 特性标注 | `{Namespace}.{SourceTypeName}.g.cs`（按模式生成的类在同一文件内） |
| `.entity.json` | `{ClassName}.entity.g.cs`（多 mode 时多个类写在同一文件） |

## 特性参考

安装包后，生成器会自动注入 `AutoCodeGenerator` 命名空间下的特性定义，无需手动引用程序集。

### AutoCodeCrudModesAttribute / AutoCodeCrud / AutoCodeRelationAttribute

见上文「推荐：CRUD 预设」。`AutoCodeCrud` 提供 mode Id 与默认 Suffix 常量；`AutoCodeCrudOmit` 为 Flags。

### AutoCodeMorphAttribute / AutoCodeAttrMacros

见上文「Morph 字段变换」。

### AutoCodeClassModesAttribute

标注在**接口或类**上，可多次使用。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | `string` | **必填**，与属性的 `Id` / `Ids` 关联 |
| `Name` | `string` | 类名，默认取源类型名 |
| `Prefix` / `Suffix` | `string` | 类名前后缀 |
| `Namespace` | `string` | 目标命名空间 |
| `NamespacePrefix` / `NamespaceSuffix` | `string` | 命名空间前后缀 |
| `Modifier` | `AccessibilityEnum` | 访问修饰符，默认取源类型可访问性 |
| `ClassStyle` | `ClassStyleEnum` | `Poco` / `Record` / `Inpc` |
| `InheritStr` / `InheritType` | `string` / `Type` | 基类；Inpc 省略时默认内置基类 |
| `InterfaceStrs` / `InterfaceTypes` | `string[]` / `Type[]` | 实现的接口 |
| `Usings` | `string[]` | 生成文件顶部的 `using` |
| `IsAbstract` | `bool` | 是否 `abstract` |
| `IsPartial` | `bool` | 是否 `partial`，默认 `true` |
| `InheritAttribute` | `bool` | 是否继承源属性上的外部 Attribute |
| `ToNullable` | `bool` | 是否将所有属性类型标记为可空 |
| `Attributes` | `string[]` | 附加到类上的 Attribute 字符串 |
| `Summary` / `Example` / `Remarks` | `string` | XML 文档 |

### AutoCodePropertyAttribute

标注在**属性**上。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` / `Ids` | `string` / `string[]` | 关联的类模式 Id |
| `Name` | `string` | 属性名，默认取源属性名 |
| `Prefix` / `Suffix` | `string` | 属性名前后缀 |
| `PropertyTypeStr` / `PropertyType` | `string` / `Type` | 覆盖属性类型 |
| `Modifier` | `AccessibilityEnum` | 访问修饰符 |
| `IsVirtual` / `IsOverride` / `IsNew` | `bool` | 虚 / 重写 / 隐藏，**三者互斥**（冲突报 `Mccj002`） |
| `IsNullable` | `bool` | 是否可空 |
| `DefaultValue` | `string` | 默认值表达式，如 `"= \"\""` |
| `IsInit` | `bool` | 生成 `init` 访问器（Poco/Record；Inpc 忽略） |
| `IsRequired` | `bool` | 生成 `required` 修饰符（Poco/Record；Inpc 忽略） |
| `Attributes` | `string[]` | 附加 Attribute |
| `Summary` / `Example` / `Remarks` | `string` | XML 文档 |

### AutoCodeNullableAttribute

标注在**接口或类**上，控制生成文件的 `#nullable` 指令（不可多次使用）。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Type` | `NullableEnum` | 见下表 |

**NullableEnum 取值：**

| 值 | 效果 |
|----|------|
| `Disable` | `#nullable disable` |
| `Enable` | `#nullable enable` |
| `Restore` | `#nullable restore` |
| `DisableAnnotations` | `#nullable disable annotations` |
| `EnableAnnotations` | `#nullable enable annotations` |
| `RestoreAnnotations` | `#nullable restore annotations` |
| `DisableWarnings` | `#nullable disable warnings` |
| `EnableWarnings` | `#nullable enable warnings` |
| `RestoreWarnings` | `#nullable restore warnings` |

### CustomAttributeAttribute

按 Id 为类或属性批量附加 Attribute 字符串，可与 `AutoCodeClassModes` / `AutoCodeProperty` 配合使用。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` / `Ids` | `string` / `string[]` | 关联的类模式 Id |
| `Attributes` | `string[]` | 要附加的 Attribute 字符串 |

### AccessibilityEnum

| 值 | 对应修饰符 |
|----|------------|
| `NotApplicable` | 无 |
| `Private` | `private` |
| `ProtectedAndInternal` | `private protected` |
| `Protected` | `protected` |
| `Internal` | `internal` |
| `ProtectedOrInternal` | `protected internal` |
| `Public` | `public` |

## 诊断

| ID | 严重性 | 说明 | 典型处理 |
|----|--------|------|----------|
| `Mccj001` | Error | 为指定类型生成代码时发生未处理异常 | 查看消息中的内部异常；向仓库反馈 |
| `Mccj002` | Error | `IsVirtual` / `IsOverride` / `IsNew` 多于一个为 true | 只保留一个修饰符 |
| `Mccj003` | Error | `.entity.json` 解析或生成失败 | 检查 JSON 语法与字段取值 |
| `Mccj004` | Warning | `AutoCodeClassModes` 缺少或为空的 Id | 为每个 ClassMode 填写唯一 `Id` |
| `Mccj005` | Warning | 属性 Id 找不到对应 ClassMode | 对齐属性与模式的 Id，或补全 ClassMode |
| `Mccj006` | Warning | 生成属性名重复被跳过 | 调整 `Name` / 前后缀避免冲突 |
| `Mccj007` | Warning | entity.json 属性缺少 name/type | 补全必填字段 |
| `Mccj008` | Warning | 有 AutoCodeProperty 但无任何有效 ClassMode | 添加带有效 Id 的 `AutoCodeClassModes` 或 `AutoCodeCrudModes` |
| `Mccj009` | Warning | `InheritStr` 类型名非法 | 检查拼写 / `nameof` |
| `Mccj010` | Warning | `Attributes` 字符串无法解析为特性 | 修正字符串；该条特性会被跳过 |
| `Mccj011` | Warning | `InterfaceTypes` 要求的成员未生成 | 在 mixin/规格上标注 `AutoCodeProperty` |
| `Mccj012` | Info | 手写标准六模式 | IDE Code Fix「改用 AutoCodeCrudModes」 |
| `Mccj013` | Warning | 索引列未出现在 Entity 投影 | 补属性或修正 Index Spec |
| `Mccj014` | Warning | `AutoCodeRelation` 缺少 `TargetName` | 填写 `TargetName = nameof(...)` |

## 从 entity.json 生成

> **双主入口 B**：声明式类型生成，与特性路径共享 Emit。当前不经 CrudModes / Relation / Morph 展开器；多 mode 与 ClassStyle 在 JSON 内直接声明。SqlSugar CRUD 六产物仍推荐入口 A（特性）。

将 JSON 文件标记为 `AdditionalFiles`，文件名以 `.entity.json` 结尾（或与 `AutoModeEntityFileSuffix` 一致）：

```xml
<AdditionalFiles Include="Models\Customer.entity.json" />
```

### 单类示例

```json
{
  "$schema": "https://raw.githubusercontent.com/mccj/AutoModeCodeGenerator/main/src/AutoModeCodeGenerator.Analyzers/EntityFiles/entity.schema.json",
  "namespace": "MyApp.Entities",
  "className": "Customer",
  "classStyle": "Poco",
  "nullable": "enable",
  "summary": "Customer entity",
  "properties": [
    { "name": "Id", "type": "int", "summary": "主键" },
    { "name": "Name", "type": "string", "isNullable": true, "isInit": true, "isRequired": true }
  ]
}
```

### 多 mode 示例

省略根级单类配置、改用 `modes` 时，可一次生成多个目标类（行为对齐特性路径的多 `AutoCodeClassModes`）：

```json
{
  "$schema": "https://raw.githubusercontent.com/mccj/AutoModeCodeGenerator/main/src/AutoModeCodeGenerator.Analyzers/EntityFiles/entity.schema.json",
  "nullable": "enable",
  "modes": [
    {
      "id": "dto",
      "namespace": "MyApp.Dtos",
      "className": "Customer",
      "suffix": "Dto",
      "classStyle": "Poco"
    },
    {
      "id": "rec",
      "namespace": "MyApp.Records",
      "className": "Customer",
      "suffix": "Rec",
      "classStyle": "Record"
    }
  ],
  "properties": [
    { "name": "Id", "type": "int", "ids": ["dto", "rec"] },
    { "name": "Name", "type": "string", "id": "dto" }
  ]
}
```

规则摘要：

- 无 `modes` 时：根级字段定义单个类（向后兼容）。
- 有 `modes` 时：每个 mode 必须有非空 `id`；类级字段可写在 mode 上，未写的可回退到根级同名字段。
- 属性未指定 `id` / `ids` 时，会进入**所有** mode；指定后仅进入匹配的 mode。
- 输出仍为单个 `{ClassName}.entity.g.cs`（默认类名取自文件名）。

NuGet 包内附带 `content/entity.schema.json`。可将该文件复制到项目中，并在 JSON 里通过相对路径引用 `$schema`，以获得编辑器补全与校验。

完整示例见 [TestModel.entity.json](https://github.com/mccj/AutoModeCodeGenerator/blob/main/src/AutoModeCodeGenerator.Samples/TestModel.entity.json)。

### JSON 字段参考

#### 根级 / 单类字段

| 字段 | 说明 |
|------|------|
| `namespace` | 目标命名空间，默认 `Generated.Entities` |
| `className` | 类名，默认取文件名（`Order.entity.json` → `Order`） |
| `nullable` | `#nullable` 指令，默认 `enable` |
| `classStyle` | `Poco`（默认）、`Record`、`Inpc` |
| `modifier` | 类访问级别：`public`（默认）、`internal`、`protected`、`private`、`protected internal`、`private protected` |
| `isAbstract` | 是否抽象类，默认 `false` |
| `isPartial` | 是否 `partial`，默认 `true` |
| `inherit` | 基类；Inpc 默认 `InpcPropertyBindableBaseAbstract` |
| `toNullable` | 为 `true` 时所有属性类型标记为可空 |
| `inheritAttribute` | 是否继承属性上的外部特性 |
| `usings` | 生成文件顶部的 `using` 列表 |
| `interfaces` | 实现的接口列表 |
| `attributes` | 类级别自定义特性（字符串，如 `global::System.Serializable`） |
| `prefix` / `suffix` | 类名前后缀 |
| `summary` / `remarks` / `example` | 类级 XML 文档 |
| `modes` | 可选多模式数组；省略时使用根级单类配置 |

#### `modes[]` 项

| 字段 | 说明 |
|------|------|
| `id` | **必填**，与 `properties[].id` / `ids` 关联 |
| 其余 | 与根级类字段同名：`namespace`、`className`、`classStyle`、`modifier`、`prefix` / `suffix`、`inherit`、`usings`、`interfaces`、`attributes`、`isAbstract`、`isPartial`、`toNullable`、`inheritAttribute`、文档字段等 |

未在 mode 上声明的字段，解析时回退到根级同名字段。

#### `properties[]` 项

| 字段 | 说明 |
|------|------|
| `name` / `type` | 属性名与类型（**必填**） |
| `id` / `ids` | 绑定到 `modes` 的 Id（无 modes 时忽略） |
| `isInit` / `isRequired` | `init` / `required`（Poco/Record；Inpc 忽略） |
| `isNullable` | 是否可空 |
| `modifier` | 属性访问级别，默认 `public` |
| `defaultValue` | 初始值表达式（如 `= \"Untitled\"`） |
| `isVirtual` / `isOverride` / `isNew` | 属性修饰符（互斥） |
| `attributes` | 属性级别自定义特性 |
| `inheritAttributes` | 属性继承特性（需配合 `inheritAttribute: true`） |
| `prefix` / `suffix` | 属性名前后缀 |
| `summary` / `remarks` / `example` | 属性级 XML 文档 |

字段完整定义见包内 `content/entity.schema.json` 或仓库 [entity.schema.json](https://github.com/mccj/AutoModeCodeGenerator/blob/main/src/AutoModeCodeGenerator.Analyzers/EntityFiles/entity.schema.json)。

## 验证与测试

仓库提供 `AutoModeCodeGenerator.Tests` 项目，通过 Roslyn `GeneratorDriver` 在内存中验证生成结果与诊断行为：

```bash
dotnet test src/AutoModeCodeGenerator.Tests/AutoModeCodeGenerator.Tests.csproj
```

主要测试类：

| 测试类 | 覆盖内容 |
|--------|----------|
| `PocoGenerationTests` | 基础 POCO、多模式生成 |
| `RecordGenerationTests` / `InpcGenerationTests` | Record 与 Inpc 样式 |
| `AdvancedGenerationTests` | 类声明源、`ToNullable`、`CustomAttribute`、默认值、可空指令等 |
| `InterfaceInheritanceTests` | 接口继承与特性复制 |
| `EntityFileGenerationTests` | `.entity.json` 字段与编译验证 |
| `AutoFilesGeneratorIntegrationTests` | 增量生成器、`Mccj003`、MSBuild 关闭开关 |
| `DiagnosticTests` | `Mccj002` 与特性模板注入 |
| `WarningDiagnosticTests` | `Mccj004`–`Mccj008` |
| `CrudProductionParityTests` | 1.x CRUD 属性集兼容、`Mccj009`/`Mccj010` |
| `CrudPresetParityTests` | `AutoCodeCrudModes` / `AutoCodeRelation` / Omit / 手写覆盖 |
| `MorphGenerationTests` | `AutoCodeMorph` File / SearchRange / StringEnum / StringArray |
| `InitRequiredAndModesTests` | `IsInit`/`IsRequired`、entity 多 mode、Inpc 默认基类 |
| `PackageContentTests` | nupkg 含 STJ 与 build props |
| `SyntaxValidationTests` | 生成代码可编译性 |
