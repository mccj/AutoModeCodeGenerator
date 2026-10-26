# Changelog

本仓库记录对消费方有意义的变更。nuget.org 已发布至 **1.0.0.17**；**1.0.1.0** 起按 AppVeyor `1.0.1.{build}` 发包（仓库基线 `1.0.1.0`）。下文 **2.2–2.5.x** 为开发里程碑标签，**未单独上 nuget.org**。

## [Unreleased]

## [1.0.1.0] - 2026-10-26

nuget.org 自 **1.0.0.17** 以来的首个新版本（含原仓库 2.2–2.6 能力）。AppVeyor 后续构建为 `1.0.1.{build}`。

### 变更（软弃用，行为保留）

- **`[Obsolete]`（`error: false`）** 引导迁移：
  - `AutoCodeClassModes` → 标准六模式用 `AutoCodeCrudModes`（自定义 mode / 与预设覆盖仍可用）
  - `InheritStr` / `InterfaceStrs` / `PropertyTypeStr` / `EntityInterfaceStrs` → 对应 `*Type(s)`
  - `CustomAttribute` → ClassModes / Property 的 `Attributes`
- 文档明确 **双主入口**：特性 CRUD 与 `.entity.json` 并列；共享 Emit（entity 路径仍不经 Expand）。

### 维护

- 删除未使用的 `BuildPropertyModifiers` / `ParseMember` / `ParseAttributeList`。
- 补齐 Extractor / EntityFiles / Emitters 管线注释；ARCHITECTURE / 升级指南 / ROADMAP 同步至 1.0.1.0。
- Analyzers 目录按管线重组：`Extraction/`、`Expansion/`、`Diagnostics/`；`SourceGeneratorHelper` → `CodeGeneration/`；`Templates/` 仅存嵌入的 `AutoCodeAttributes.cs`（行为不变）。

## [2.5.1] - 2026-10-26

### 新增

- **`AutoCodeCrudModes.SearchRangeType`**：可用 `typeof(SearchRangeForStructValue<>)` 指定区间开泛型；与既有 `SearchRangeTemplate` 字符串兼容（字符串优先，其次 Type，再 MSBuild）。
- **`AutoCodeMorph.RangeType`**：Morph.SearchRange 同样支持 `typeof(...)`；与 `RangeTypeTemplate` 字符串兼容。

### 文档

- Analyzers README / 升级指南示例补充 `SearchRangeType`。

## [2.5.0] - 2026-10-25

### 新增

- **Detail/Dto 去重**：Create/Update/Detail 继承 Dto 且同名同类型时不再重复投影，避免 CS0108。
- **`InjectMarkerMembers`**：`AutoCodeCrudModes` / `AutoCodeClassModes` 可选；自动为 `InterfaceTypes` 未投影成员补 Entity/Dto（DateTime 等进 Search 区间）。
- **`SearchRangeTemplate`** + MSBuild **`AutoModeSearchRangeTemplate`**：Morph.SearchRange / 注入共用默认区间类型（如 `SearchRangeForStructValue<{0}>`）。
- **Mccj014**：`AutoCodeRelation` 的 `TargetName` 为空时 Warning（不再静默跳过）。
- **`SuggestCrudModesAnalyzer` + CodeFix**：Mccj012 可由 IDE「改用 AutoCodeCrudModes」一键替换六行 ClassModes。
- **DateTime 转换器宏**：`AutoCodeAttrMacros.SugarColumnDateTimeDbType`、`SqlSugarAttrTemplates.DateTimeConverter(...)`。

### 文档

- 升级说明、AI 指南、Conventions 模板、Analyzers README 同步至 2.5。
- 新增 [docs/ROADMAP.md](docs/ROADMAP.md)（2.5 之后建议与非目标）；根 README / docs 索引按角色与版本能力刷新。

## [2.4.0] - 2026-10-25

### 新增

- **`AutoCodeColumn`**：属性级列约束短语法（`Length` / `Max` / `BigString` / `Name`），合并到 Entity 投影。
- **`AutoCodeIndex`**：类级索引声明（`Spec` 或 `Columns`+`Orders` / `Unique`），展开为 `SugarIndex(...)`。
- **`AutoCodeModeFlags`** / `AutoCodeProperty.ModeFlags`：压缩常见 Id 组合（如 `EntityDtoSearch`）。
- **`AutoCodeClassModes.InheritDto`**：Create/Update/Detail 无需手写 `InheritStr = nameof(X)+"Dto"`。
- **`AutoCodeAttrMacros`** 扩充：`StringLength64/128/1024/Max`、`SugarBigString`、`OpenApiStringEnum` 等。
- 诊断 **`Mccj011`**：`InterfaceTypes` / `EntityInterfaceTypes` 要求的成员未生成属性（典型：`ICreateTimeEntity.CreateTime`）。
- 诊断 **`Mccj012`**（Info）：手写标准六模式时建议改用 `AutoCodeCrudModes`。
- 诊断 **`Mccj013`**：`AutoCodeIndex` 引用的列未出现在 Entity 投影。
- Conventions.SqlSugar：长度/BigString/索引构建器、CreateTime 与 Entity-only mixin 文档片段。

### 文档

- 更新 AI 指南、Conventions 模板、升级说明与改进计划（阶段 H / 2.4）。

## [2.3.0] - 2026-10-25

### 新增

- **`AutoCodeMorph`** / **`AutoCodeMorphKind`**：`File` / `SearchRange` / `StringEnum` / `StringArray` 按 mode 展开类型；`EntityAttributes` / `ApiAttributes` 可覆盖特性宏；类型字符串均可项目级覆盖。
- **`AutoCodeAttrMacros`**：常用特性字面量常量（含可选 SqlSugar / STJ 示例，主包仍无 ORM 程序集依赖）。
- 文档模板：[docs/CONVENTIONS_TEMPLATE.md](docs/CONVENTIONS_TEMPLATE.md)。
- 可选包 **`AutoModeCodeGenerator.Conventions.SqlSugar`**：SqlSugar 字符串模板（不引用 SqlSugar.dll）。

### 测试

- `MorphGenerationTests`。

## [2.2.0] - 2026-10-25

### 新增

- **`AutoCodeCrudModes`** / **`AutoCodeCrud`** / **`AutoCodeCrudOmit`**：一次声明展开 Entity/Dto/Create/Update/Detail/Search；与手写 `AutoCodeClassModes` 同 Id 时手写优先。
- **`AutoCodeRelation`**：外键展开为 Id / Detail / Search /（可选）Navigate；默认 Navigate 模板为 SqlSugar 字符串，可关闭或替换。
- 诊断 **`Mccj009`**（`InheritStr` 非法）、**`Mccj010`**（`Attributes` 无法解析）；无效 Attributes 不再导致生成器抛异常。

### 文档与示例

- 升级指南：[docs/UPGRADE_1x_TO_2x.md](docs/UPGRADE_1x_TO_2x.md)。
- Samples `CrudCookbook` 改为预设 + Relation；消费方 README 以预设为主路径。
- [docs/IMPROVEMENT_PLAN.md](docs/IMPROVEMENT_PLAN.md)（阶段 A–D 已落地）。

### 测试

- `CrudProductionParityTests`、`CrudPresetParityTests`。

## [2.0.0] - 2026-10-24

### 破坏性变更

- **`ClassStyle.Record`**：生成真正的 `record class`，属性为 `{ get; init; }`（此前为普通 `class` + `{ get; }`）。
- **`InpcPropertyBindableBaseAbstract`**：实现 `INotifyPropertyChanged`；`SetValue` 在值变化时触发 `PropertyChanged`。生成的 Inpc setter 仅调用 `SetValue`，不再额外调用 `RaisePropertyChanged()`。
- **内部命名空间**：`自动代码生成` 重命名为 `AutoModeCodeGenerator.Internal`（仅影响测试与内部引用，不影响 `AutoCodeGenerator` 公共 API）。

### 新增

- 诊断 Warning：`Mccj004`–`Mccj008`（空 Id、未绑定 mode、重复属性名、JSON 缺字段、无有效 ClassMode 等）。
- 属性选项：`IsInit` / `IsRequired`（特性与 `.entity.json`；Poco/Record 生效，Inpc 忽略）。
- `.entity.json` 支持 `modes[]` 多模式生成；属性可用 `id` / `ids` 绑定。
- MSBuild 开关：`AutoModeGenerateEntityFiles`、`AutoModeEntityFileSuffix`（见包内 `build/AutoModeCodeGenerator.Analyzers.props`）。
- NuGet 包内打包 `System.Text.Json` 及其传递依赖，保障消费端 `AutoFilesGenerator` 可加载。
- Inpc 未指定 `InheritType` / `inherit` 时，默认继承 `InpcPropertyBindableBaseAbstract`。

### 文档与工程

- 消费方 README、根 README、PACKAGING 同步至 2.0.0。
- CI：版本一致性校验、nupkg 包内容断言。

## [1.0.0.18] 及更早

见 NuGet 历史版本与 Git 提交记录。
