# AI 消费方技术指南（AutoModeCodeGenerator）

> **给其他 AI / Agent 的指令文档。** 人类可读摘要见 [Analyzers README](../src/AutoModeCodeGenerator.Analyzers/README.md)。  
> 目标版本：**1.0.1.0+**。命名空间：`AutoCodeGenerator`。包：`AutoModeCodeGenerator.Analyzers`（`PrivateAssets="all"`）。

---

## 0. 30 秒定位

| 问题 | 答案 |
|------|------|
| 这是什么？ | Roslyn **编译期源生成器**：特性规格或 `.entity.json` → 生成 POCO / Record / Inpc |
| 双主入口？ | **A** 接口+特性（CRUD 六产物）；**B** `.entity.json`（声明式类型）。共享 Emit |
| CRUD 默认应写什么？ | `[AutoCodeCrudModes]` + `[AutoCodeProperty]` + 可选 Relation / Morph / Column |
| 不要默认写什么？ | 手写六行 `AutoCodeClassModes`（1.0.1 Obsolete）、`*Str` 属性、四段式 FK |
| ClassStyle？ | 默认 **Poco**；`Record` / `Inpc` 按需选用（非「禁用」） |
| 生成时机？ | **编译时**；无运行时 API、无反射配置 |
| 特性从哪来？ | PostInitialization **自动注入**；消费方**不要**手写/复制 `AutoCodeAttributes.cs` |

### 0.1 使用目的（Agent 必须对齐）

消费方把本库当成 **「接口驱动的 SqlSugar / Admin.NET CRUD 多产物生成器」**，目标是：

1. **一份接口规格 → 六套类型**：Entity、Dto、CreateInput、UpdateInput、DetailOutput、SearchInput  
2. **按模式投影字段**：同一概念可只进部分产物（业务字段进 Entity/Dto/Search，导航进 Detail 等）  
3. **对接现有栈**：生成结果交给 SqlSugar ORM + `BaseCrudAppService` / 手写 Service（**本库不生成** Controller / AppService / Migration）  
4. **消灭样板**：六行 ClassModes、外键四段式、字符串特性堆叠、跨实体 Const/Mixin 复制  

**定位一句：** 用接口声明领域与投影规则，换统一、可维护的 CRUD 类型族；不是通用 POCO/Record/JSON 建模工具。

### 0.2 消费方使用架构

```text
源接口 (EntitySpecs/*.cs)
  + 共享 mixin（IHasLongId / 审计 / 软删）
  + AutoCodeCrudModes + Relation / Morph / Column / Index
        │  编译期展开
        ▼
Entity(*Record|*Entity) + Dto + CreateInput + UpdateInput + DetailOutput + SearchInput
        │
        ▼
SqlSugar 映射 + BaseCrudAppService / 手写 Service（消费方自写）
```

| 层 | 角色 | Agent 注意 |
|----|------|------------|
| **规格层** | 带 `AutoCode*` 的接口；mixin 承载 Id/审计；可选 Const | **只改规格**，不要改生成类 |
| **展开层** | CrudModes / Relation / Morph / Column / Inject | 用声明式 API，勿手写四段式 |
| **产物层** | Entity 进库；Dto 作 Create/Update/Detail 基类；Search 默认可空 | Entity 后缀默认 `Record`（类名，**不是** `ClassStyle`） |
| **消费层** | ORM + CRUD 服务 | 本库边界止于数据形状 |

标准六模式职责：

| Mode | 典型职责 |
|------|----------|
| Entity | 表映射；`EntityInterfaceTypes` / `SugarTable` |
| Dto | Create/Update/Detail 的继承基类 |
| Create / Update / Detail | 默认 `InheritDto`；Detail 侧重导航可空 |
| Search | 默认 `ToNullable`；区间等 Morph / `PropertyType` |

### 0.3 使用优势（写规格时心里要有的收益）

1. **一处规格，多产物** — 避免六套手写类型漂移。  
2. **按模式投影** — `Ids` / `ModeFlags` 控制字段进哪些产物，CRUD 边界清晰。  
3. **贴合现有栈** — 产物直接对接 SqlSugar + Admin.NET 式 CRUD，不另搞模型体系。  
4. **横切可复用** — mixin + 约定常量跨实体复用，比复制整类稳。  
5. **搜索/表单语义内建** — Search 可空、InheritDto、Relation、Morph（文件/枚举/区间）。  
6. **样板更少（1.0.1）** — CRUD 预设、Relation、Morph、Column/Index、Inject、诊断/CodeFix。  

Agent 选型时：**优先声明式预设路径**，把上述优势落成代码；不要退回六行 ClassModes / 外键四段式，除非维护旧代码（§6）。

---

## 1. 决策树（必须先走这里）

```
需要生成什么？
├─ SqlSugar/Admin.NET CRUD 六产物？ → [AutoCodeCrudModes]（§3）
│         外键？ → [AutoCodeRelation]（§4）+ TargetName 必填（否则 Mccj014）
│         列长/索引？ → [AutoCodeColumn] / [AutoCodeIndex]（§4.5）
│         文件/枚举/区间/CSV？ → [AutoCodeMorph]（§5）
│         审计标记？ → InjectMarkerMembers 或 IHasCreateTime mixin（§4.5）
│         旧六行 ClassModes？ → CS0618；优先 Code Fix→CrudModes（Mccj012）
├─ 声明式 JSON 类型（入口 B）？ → .entity.json（与特性并列主路径；无 CrudModes 展开）
├─ 单个自定义 mode / 非标准 CRUD？ → 少量 [AutoCodeClassModes]（§6，Obsolete 但可用）
└─ WPF/绑定 INPC？ → ClassStyle.Inpc（按需）
```

**硬规则：** CRUD 六产物一律 `[AutoCodeCrudModes]`，默认 **Poco**；勿新写六行 ClassModes / `*Str` / 四段式 FK。entity.json 为并列主入口，勿降级为「禁止」；Record/Inpc 仅在用户需要时选用。

属性 Ids 优先 `ModeFlags`（如 `EntityDtoSearch`）；Detail 继承 Dto 时同类型字段用 `EntityDto` 即可（生成器也会去重）。

---

## 2. 安装与工程约定

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.1.0" PrivateAssets="all" />
```

可选（便于 AI/人查看生成结果）：

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
```

| 注意 | 说明 |
|------|------|
| `PrivateAssets="all"` | **必须**：避免分析器 DLL 传到依赖链运行时 |
| 勿引用 Analyzers 为普通 `ProjectReference` 且 `ReferenceOutputAssembly=true` | 会污染运行时；本地开发用 `OutputItemType="Analyzer"` + `ReferenceOutputAssembly="false"` |
| 主包无 ORM 依赖 | SqlSugar 字符串只是模板；项目需自行引用 SqlSugar |
| 可选约定包 | `AutoModeCodeGenerator.Conventions.SqlSugar`（仅字符串模板，无 SqlSugar.dll） |
| Search 区间默认类型 | `SearchRangeType = typeof(…<>)` / `SearchRangeTemplate` 字符串 / MSBuild `AutoModeSearchRangeTemplate`（优先级：字符串 → Type → MSBuild） |

```xml
<AutoModeSearchRangeTemplate>SearchRangeForStructValue&lt;{0}&gt;</AutoModeSearchRangeTemplate>
```

生成文件名：`{Namespace}.{SourceTypeName}.g.cs`（同一源类型的多 mode 在同一文件）。

---

## 3. 标准模板：CRUD 预设（优先复制此块）

```csharp
using AutoCodeGenerator;

namespace MyApp.EntitySpecs;

/// <summary>框架实体标记（按项目替换为 IEntity&lt;long&gt; 等）。</summary>
public interface IEntityMarker { }

public interface IHasLongId
{
    // Entity 有 Id；Update/Detail 也需要 Id；Create/Dto/Search 通常不要
    [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
    [AutoCodeProperty(Ids = [AutoCodeCrud.Update, AutoCodeCrud.Detail])]
    long Id { get; set; }
}

[AutoCodeNullable(Type = NullableEnum.Enable)]
[AutoCodeCrudModes(
    EntityInterfaceTypes = [typeof(IEntityMarker)],
    EntityAttributes = ["global::SqlSugar.SugarTable(\"product\")"] // 可选
)]
public interface Product : IHasLongId
{
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search])]
    string Name { get; set; }

    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto])]
    decimal Price { get; set; }

    [AutoCodeRelation(TargetName = nameof(Category), IncludeNavigate = true)]
    long Category { get; set; }
}
```

### 3.1 预设默认展开（勿改 Id 字符串）

| `AutoCodeCrud.*` 常量 | Id 字面量 | 默认后缀 | 行为 |
|----------------------|-----------|----------|------|
| `Entity` | `"Entity"` | `Record` | 可挂 `EntityInterfaceTypes` / `EntityAttributes` |
| `Dto` | `"Dto"` | `Dto` | Create/Update/Detail 默认 `InheritStr = {Name}Dto` |
| `Create` | `"CreateInput"` | `CreateInput` | SummaryPrefix「创建」 |
| `Update` | `"UpdateInput"` | `UpdateInput` | SummaryPrefix「更新」 |
| `Detail` | `"DetailOutput"` | `DetailOutput` | SummaryPrefix「详情」 |
| `Search` | `"SearchInput"` | `SearchInput` | **`ToNullable = true`**；SummaryPrefix「搜索」 |

生成类型名示例（源接口 `Product`）：

- `ProductRecord`、`ProductDto`、`ProductCreateInput`、`ProductUpdateInput`、`ProductDetailOutput`、`ProductSearchInput`

### 3.2 常用选项

```csharp
// 内控等项目 Entity 后缀不是 Record：
[AutoCodeCrudModes(EntitySuffix = "Entity")]

// 仅生成实体（关联表）：
[AutoCodeCrudModes(OmitModes = AutoCodeCrudOmit.EntityOnly)]

// 无 Update：
[AutoCodeCrudModes(OmitModes = AutoCodeCrudOmit.NoUpdate)]

// Create/Update/Detail 不继承 Dto：
[AutoCodeCrudModes(CreateUpdateDetailInheritDto = false)]
```

### 3.3 合并规则（极易踩坑）

- 同一源类型可同时有 `AutoCodeCrudModes` + 手写 `AutoCodeClassModes`。
- **相同 `Id`：手写覆盖预设。**
- 属性 `Ids` **必须**使用 `AutoCodeCrud.*` 常量（或与之一致的字符串）；自造 `"entity"` / `"EntityId"` 会触发 `Mccj005`。

### 3.4 字段进哪些产物？（选型表）

| 字段语义 | 推荐 Ids / ModeFlags |
|----------|----------|
| 业务可写字段（名称、金额等） | `ModeFlags = EntityDtoSearch` |
| 大文本 / 不进搜索 | `ModeFlags = EntityDtoDetail` + `[AutoCodeColumn(BigString = true)]` |
| 仅持久化、不进表单 | `Entity`（+ 可选 `Detail`） |
| 仅详情展示 | `Detail` 或经 Dto 继承 |
| 主键 Id | `Entity` + `Update` + `Detail`（见 mixin） |
| 审计 CreateTime | mixin `IHasCreateTime` + `EntityInterfaceTypes = ICreateTimeEntity<>`（缺属性 → **Mccj011**） |
| 外键 | 用 `AutoCodeRelation`，不要手写四段式 |

**Create/Update/Detail 默认继承 Dto**：标在 `Dto` 上的属性会进入三者；不必再写 `Create`/`Update`/`Detail` Ids，除非类型与 Dto 不同（Morph / Relation 已处理）。

---

## 4. 外键：`AutoCodeRelation`

```csharp
[AutoCodeRelation(TargetName = nameof(Category))]  // IncludeNavigate 默认 true
long Category { get; set; }
```

### 4.1 展开结果（属性名 `Category`，目标 `Category`）

| 生成成员 | 所在 mode | 类型（默认后缀） |
|----------|-----------|------------------|
| `CategoryId` | Entity, Dto, Search | 源属性类型（如 `long`） |
| `Category` | Detail | `CategoryDetailOutput?` |
| `Category` | Search | `CategorySearchInput?` |
| `Category` | Entity | `CategoryRecord?` + Navigate 特性 |

Navigate 默认模板（SqlSugar 字面量，**不引用** SqlSugar 程序集）：

```text
global::SqlSugar.Navigate(global::SqlSugar.NavigateType.ManyToOne, nameof({0}Id))
```

`{0}` = 源属性名（如 `Category` → `nameof(CategoryId)`）。

### 4.2 技巧与注意

| 技巧 | 说明 |
|------|------|
| 属性类型写 FK 标量 | `long Category`，**不要**写成 `CategoryRecord` |
| `TargetName` 用 `nameof` | 指向**规格接口名**（生成器拼 `TargetName + Suffix`） |
| 无 SqlSugar 时 | `IncludeNavigate = false`（Samples 即如此） |
| EF / 其它 ORM | 改 `NavigateAttribute` 模板字符串 |
| 与手写 Property 并存 | 勿对同一属性再写四段式，易 `Mccj006` 重名跳过 |
| `TargetName` 为空 | Relation **静默跳过**（无诊断）——务必赋值 |

---

## 4.5 列 / 索引 / ModeFlags

```csharp
[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(ICreateTimeEntity<long>)])]
[AutoCodeIndex("IX_Raw_CreateTime", Spec = "CreateTime:Desc")]
[AutoCodeIndex(Name = "UK_DeviceAttr", Unique = true, Columns = ["ProductId", "DeviceName"])]
public interface InboundRaw : IHasLongId, IHasCreateTime
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    [AutoCodeColumn(Length = 64)]
    string? ProductId { get; set; }

    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoDetail)]
    [AutoCodeColumn(BigString = true)]
    string? PlatformWire { get; set; }
}
```

| API | 作用 |
|-----|------|
| `ModeFlags` | 等价填写 `Ids`；可与 `Id`/`Ids` 并集 |
| `AutoCodeColumn` | 合并到 **Entity** 的 StringLength / BigString / ColumnName |
| `AutoCodeIndex` | 附加到 Entity 类的 `SugarIndex`；列名须已投影，否则 **Mccj013** |
| `InheritDto = true` | 手写 ClassModes 时免写 `InheritStr = nameof(X)+"Dto"` |

**硬规则：** `EntityInterfaceTypes` / `InterfaceTypes` 只决定生成类 `:` 后面的接口列表；**属性必须来自** 规格或 mixin 上的 `AutoCodeProperty`。只挂 `ICreateTimeEntity` 不声明 `CreateTime` → **Mccj011** + 编译 CS0535。

**Inject / SearchRange 捷径：**

```csharp
[AutoCodeCrudModes(
    EntityInterfaceTypes = [typeof(ICreateTimeEntity<long>)],
    InjectMarkerMembers = true, // 自动投影 CreateTime + Search 区间
    SearchRangeType = typeof(SearchRangeForStructValue<>))]
public interface DeviceBootLogs : IHasLongId { /* 业务字段 */ }
```

或字符串 `SearchRangeTemplate`；或项目级 MSBuild：`<AutoModeSearchRangeTemplate>SearchRangeForStructValue&lt;{0}&gt;</AutoModeSearchRangeTemplate>`。

Detail 继承 Dto 且类型相同时，生成器会去重 Detail 属性（避免 CS0108）。手写六模式可用 IDE **Code Fix「改用 AutoCodeCrudModes」**（Mccj012）。

完整 mixin 模板：[CONVENTIONS_TEMPLATE.md](CONVENTIONS_TEMPLATE.md)。

---

## 5. 双轨字段：`AutoCodeMorph`

| Kind | Entity | Dto（Create/Update 继承） | Detail | Search |
|------|--------|---------------------------|--------|--------|
| `File` | `string` | `FileCreateInput` | `FileOutput`（异于 Dto 才单独投影） | — |
| `StringEnum` | `string` | 枚举名 | 继承 Dto | 枚举 |
| `SearchRange` | 需另标 Property | — | — | `SearchRange<T>`（可改模板） |
| `StringArray` | `string` | `string[]` | 默认同数组 | — |

```csharp
// 文件：按项目覆盖类型名
[AutoCodeMorph(Kind = AutoCodeMorphKind.File,
    CreateUpdateType = "UploadFileInput", DetailType = "FileOutput")]
string Receipt { get; set; }

// 枚举双轨 + 特性宏
[AutoCodeMorph(Kind = AutoCodeMorphKind.StringEnum, EnumType = nameof(PayStatus),
    EntityAttributes = [AutoCodeAttrMacros.SugarColumnIgnore],
    ApiAttributes = [AutoCodeAttrMacros.JsonIgnore])]
PayStatus Status { get; set; }

// Search 区间：Entity/Dto 仍用普通 Property
[AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto])]
[AutoCodeMorph(Kind = AutoCodeMorphKind.SearchRange)]
DateTime Created { get; set; }

// CSV ↔ 数组
[AutoCodeMorph(Kind = AutoCodeMorphKind.StringArray)]
string Tags { get; set; }
```

**注意：**

- Morph 展开使用的 mode Id 是固定字面量 `"Entity"` / `"Dto"` / `"DetailOutput"` / `"SearchInput"`，与 `AutoCodeCrud` 一致；手写 ClassModes 若改了 Id，Morph **对不上**。
- `SearchRange` **只**往 Search 投影；Entity/Dto 必须另写 `[AutoCodeProperty]`。
- 默认类型名（`FileCreateInput`、`SearchRange<T>`）是约定字符串，项目需自备同名类型或覆盖 `*Type` / `RangeTypeTemplate`。

---

## 6. 手写六模式（兼容 / 非 CRUD）

仅当：维护旧代码、或需要非标准 mode。新实体优先 §3。

```csharp
[AutoCodeClassModes(Id = AutoCodeCrud.Dto, Suffix = AutoCodeCrud.DefaultDtoSuffix)]
[AutoCodeClassModes(Id = AutoCodeCrud.Entity, Suffix = AutoCodeCrud.DefaultEntitySuffix,
    InterfaceTypes = [typeof(IEntityMarker)])]
[AutoCodeClassModes(Id = AutoCodeCrud.Create, Suffix = AutoCodeCrud.DefaultCreateSuffix,
    InheritDto = true, SummaryPrefix = "创建")]
[AutoCodeClassModes(Id = AutoCodeCrud.Update, Suffix = AutoCodeCrud.DefaultUpdateSuffix,
    InheritDto = true, SummaryPrefix = "更新")]
[AutoCodeClassModes(Id = AutoCodeCrud.Detail, Suffix = AutoCodeCrud.DefaultDetailSuffix,
    InheritDto = true, SummaryPrefix = "详情")]
[AutoCodeClassModes(Id = AutoCodeCrud.Search, Suffix = AutoCodeCrud.DefaultSearchSuffix,
    ToNullable = true, SummaryPrefix = "搜索")]
public interface Product { /* ... */ }
```

`InheritStr` / `PropertyTypeStr` 指向**尚未生成**的类型时只能拼字符串（如 `nameof(X) + "Dto"`）；非法名会 `Mccj009`。

---

## 7. AI 高频错误清单（生成前自检）

| # | 错误 | 正确做法 |
|---|------|----------|
| 1 | 把 Entity 后缀 `Record` 当成 `ClassStyle.Record` | 后缀只是类名；样式保持默认 Poco |
| 2 | 新实体手写六行 ClassModes | 用 `[AutoCodeCrudModes]` |
| 3 | 外键手写四条 Property | 用 `[AutoCodeRelation]` |
| 4 | `Ids = ["EntityId"]` 等自造 Id | 用 `AutoCodeCrud.Entity` 等 |
| 5 | 在 Create 上重复标已在 Dto 的字段 | 依赖 `CreateUpdateDetailInheritDto` |
| 6 | `Attributes` 写 C# 表达式而非字符串 | 必须是可解析特性字面量，建议 `global::...` |
| 7 | 复制嵌入模板源码到业务项目 | 包会注入；复制会导致类型冲突 |
| 8 | 忘记 `using AutoCodeGenerator;` | 编译失败或找不到特性 |
| 9 | Relation 的 `TargetName` 写成已生成类名 `CategoryRecord` | 写规格名 `Category` |
| 10 | 默认 `IncludeNavigate=true` 但未引用 SqlSugar | 编译 Entity 失败 → 设 `false` 或加引用 |
| 11 | 把生成类当源规格再标特性 | 只在**接口规格**上标；生成类是产物 |
| 12 | 同一属性 `IsVirtual`+`IsOverride` | 互斥，否则 `Mccj002` Error |
| 13 | mixin 基接口无 `AutoCodeProperty` | 继承成员只有带特性的才参与生成 |
| 14 | 期望运行时改生成结果 | 不可能；改规格后**重新编译** |
| 15 | 新代码继续写六行 ClassModes / `*Str` | 改用 CrudModes / `*Type`（1.0.1 Obsolete） |

---

## 8. 诊断速查（Mccj）

| ID | 级 | 含义 | AI 应做什么 |
|----|----|------|-------------|
| `Mccj001` | E | 生成器内部异常 | 收敛复现；查 Attributes 字符串；反馈仓库 |
| `Mccj002` | E | virtual/override/new 冲突 | 只留一个 |
| `Mccj003` | E | entity.json 失败 | 修 JSON / schema |
| `Mccj004` | W | ClassModes 空 Id | 填唯一 Id |
| `Mccj005` | W | 属性 Id 无对应 mode | 对齐 `AutoCodeCrud.*` 或补 ClassModes |
| `Mccj006` | W | 生成属性名重复被跳过 | 查 Relation+手写重复、Suffix 冲突 |
| `Mccj007` | W | JSON 属性缺 name/type | 补字段 |
| `Mccj008` | W | 有 Property 无有效 mode | 加 `AutoCodeCrudModes` 或 ClassModes |
| `Mccj009` | W | InheritStr 非法 | 检查 `nameof` 拼接 |
| `Mccj010` | W | Attributes 无法解析 | 修字符串；该条跳过，其余继续 |
| `Mccj011` | W | InterfaceTypes 成员未生成 | 在 mixin/规格上标 `AutoCodeProperty`（如 CreateTime） |
| `Mccj012` | I | 建议改用 AutoCodeCrudModes | IDE Code Fix「改用 AutoCodeCrudModes」 |
| `Mccj013` | W | 索引列不在 Entity 投影 | 补属性或改 Index Spec |
| `Mccj014` | W | Relation 缺 TargetName | 填写 `TargetName = nameof(...)` |

验证手段：

1. 开启 `EmitCompilerGeneratedFiles`，读 `obj/.../Generated/.../*.g.cs`。
2. `dotnet build` 看 Mccj 诊断。
3. 对照 Samples：`src/AutoModeCodeGenerator.Samples/CrudCookbook/CrudProduct.cs`。

---

## 9. 字符串特性编写规范

`Attributes` / `EntityAttributes` / `NavigateAttribute` / Morph 宏均为 **字符串**，在生成代码中原样嵌入为特性。

**推荐：**

```csharp
Attributes = ["global::SqlSugar.SugarColumn(IsPrimaryKey = true)"]
Attributes = [$"global::SqlSugar.Navigate(global::SqlSugar.NavigateType.ManyToOne, nameof({nameof(Category)}Id))"]
```

**避免：**

- 缺少 `global::` 导致生成文件缺 using 而编译失败（除非 ClassModes.`Usings` 已补）。
- 未转义的引号弄断字符串。
- 把多条特性塞进一个字符串（应使用 `string[]` 多元素）。

内置宏：`AutoCodeAttrMacros.Serializable` / `SugarPrimaryKey` / `SugarColumnIgnore` / `JsonIgnore`。

---

## 10. Mixin / 约定（减少样板）

推荐结构：

1. `IHasLongId` / `IHasAudit` / `IHasSoftDelete` — 仅属性 + `AutoCodeProperty`
2. 业务接口：`[AutoCodeCrudModes]` + 继承 mixin
3. 项目级 Const 仅当需要别名；优先直接用 `AutoCodeCrud`

完整可复制模板：[CONVENTIONS_TEMPLATE.md](CONVENTIONS_TEMPLATE.md)。

树节点等变体：

- 基类用 `InheritType = typeof(TreeNodeBase)`（手写覆盖 Entity mode）
- 父级 FK 覆盖基类成员：`IsOverride = true`
- Entity 后缀统一用 `EntitySuffix`，不要混用 `Record`/`Entity` 除非项目已有约定

---

## 11. Agent 工作流（建议步骤）

当用户要求「用本库生成某某实体」时：

1. **确认包版本** ≥ **1.0.1.0**；否则先升包（见 [UPGRADE.md](UPGRADE.md)）。
2. **找项目惯例**：Entity 后缀？`IEntity<T>`？表名特性？有无现成 mixin？
3. **写规格接口**（§3 模板），字段按 §3.4 选 Ids。
4. **外键 / Morph** 用 §4 / §5；禁止发明新四段式。
5. **编译**；若失败读生成 `.g.cs` 与 Mccj。
6. **不要**提交 `obj/Generated`；不要改 Analyzers 源码来「修业务」。
7. 服务层 / Api 仍手写；本库**只生成数据形状**，不生成 AppService。

### 最小 diff 原则

- 改一个实体：只动该接口规格文件。
- 新实体：优先复用项目已有 mixin / Const，勿新建第二套 Id 体系。
- 迁移旧六模式：可分批；升包与改预设分开提交更安全。

---

## 12. 能力边界（勿承诺）

| 会做 | 不会做 |
|------|--------|
| 多 mode POCO / 可选 Record·Inpc | 生成 DbContext、Migration、Controller |
| 字符串特性嵌入 | 编译期验证 SqlSugar API 是否存在 |
| 接口继承合并 Property | 跨程序集「改已编译类型」 |
| 增量生成（改规格才重跑） | 运行时动态生成 |
| entity.json（入口 B，声明式） | 在 JSON 内自动展开 CrudModes/Relation（当前不支持） |

ORM / 框架集成由消费方完成；生成器保持 **ORM 无关**。

---

## 13. 参考锚点（深入时打开）

| 文档 / 代码 | 用途 |
|-------------|------|
| [Analyzers README](../src/AutoModeCodeGenerator.Analyzers/README.md) | 完整特性与 entity.json 参考 |
| [UPGRADE.md](UPGRADE.md) | 1.0.0.x → 1.0.1.x 迁移 |
| [CONVENTIONS_TEMPLATE.md](CONVENTIONS_TEMPLATE.md) | mixin / 宏模板 |
| [ROADMAP.md](ROADMAP.md) | 1.0.1 之后建议与非目标 |
| `Samples/CrudCookbook/` | 可编译金标准（含 InboundRaw 示例） |
| `Templates/AutoCodeAttributes.cs` | 注入 API 真源（勿复制到业务） |
| `CrudPresetExpander.cs` / `MorphExpander.cs` / `ColumnIndexExpander.cs` | 展开语义真源 |

---

## 14. 一页速记（可贴进 Agent system prompt）

```
你在使用 AutoModeCodeGenerator.Analyzers 1.0.1+（ns: AutoCodeGenerator）。
双主入口：A 特性 CRUD；B .entity.json 声明式；共享 Emit。
CRUD 目的：一份规格 → Entity/Dto/Create/Update/Detail/Search；只生成数据形状。
架构 A：规格接口+mixin → 编译展开 → 六产物 → 消费方 SqlSugar+AppService。
CRUD 主路径：接口 + [AutoCodeCrudModes] + [AutoCodeProperty(ModeFlags=…)]。
外键：[AutoCodeRelation(TargetName=nameof(X))]；列/索引：[AutoCodeColumn]/[AutoCodeIndex]。
双轨：[AutoCodeMorph]；审计可用 InjectMarkerMembers 或 IHasCreateTime mixin。
默认 Poco；Entity 后缀默认 "Record"（不是 ClassStyle.Record）。
Create/Update/Detail 默认继承 *Dto；Search 默认 ToNullable。
1.0.1：ClassModes/*Str/CustomAttribute 为 Obsolete；新代码勿写六行 ClassModes。
entity.json 是并列主入口（无 CrudModes 展开）；Record/Inpc 按需。
TargetName 必填（Mccj014）；六行 ClassModes 用 Mccj012 Code Fix。
Attributes 用 global:: 字符串。改规格后编译看 *.g.cs。不生成 Controller/Service。
```

深入：本文 §0（目的/架构/优势）· [ROADMAP.md](ROADMAP.md)（非目标）· [CONVENTIONS_TEMPLATE.md](CONVENTIONS_TEMPLATE.md)
