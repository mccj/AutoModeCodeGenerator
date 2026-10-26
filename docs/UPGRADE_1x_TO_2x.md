# 从 1.0.0.9–1.0.0.17 升级到 1.0.1.x

面向仍钉在旧包的生产项目。建议路径：**先升到 1.0.1.0（或 AppVeyor `1.0.1.{build}`）验证 CRUD 输出 → 再按 CS0618 / Mccj012 逐步改用预设 / Relation / Column / Index / Inject**。

能力对照见 [docs/README.md](README.md)「版本能力速查」；后续规划见 [ROADMAP.md](ROADMAP.md)。（仓库曾用 2.2–2.6 作里程碑标签，**未上 nuget.org**；正式包号为 1.0.1.x。）

## 1. 升包（最小变更）

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.1.0" PrivateAssets="all" />
```

可选约定包：

```xml
<PackageReference Include="AutoModeCodeGenerator.Conventions.SqlSugar" Version="1.0.1.0" />
```

1. 还原并全量编译。
2. 对比生成产物：Entity(`*Record`) / Dto / Create / Update / Detail / Search 类型名、继承、`ToNullable`、接口与字符串 `Attributes`。
3. 若使用 `ClassStyle.Record` 或 Inpc：阅读 [CHANGELOG 2.0.0](../CHANGELOG.md) 破坏性说明。
4. 多数 CRUD 项目默认 **Poco**，升 2.x **无破坏**（生成行为保留）。
5. 留意新 Warning：**Mccj011**（接口成员未投影）、**Mccj013**（索引列）、**Mccj014**（Relation 空 TargetName）；**Mccj012** 可用 Code Fix 一键改成 `AutoCodeCrudModes`。
6. **1.0.1 CS0618**：`AutoCodeClassModes` / `*Str` / `CustomAttribute` 等带 `[Obsolete]`（`error: false`）。见下表。

兼容回归由仓库 `CrudProductionParityTests` 覆盖。

## 2. 2.6 Obsolete 迁移表（CS0618）

| 旧 API | 改用 | 说明 |
|--------|------|------|
| `AutoCodeClassModes`（标准六行） | `AutoCodeCrudModes` | 自定义 mode / 与预设同 Id 覆盖时仍可保留 ClassModes |
| `InheritStr` | `InheritType` | |
| `InterfaceStrs` | `InterfaceTypes` | |
| `PropertyTypeStr` | `PropertyType` | |
| `EntityInterfaceStrs` | `EntityInterfaceTypes` | 在 `AutoCodeCrudModes` 上 |
| `CustomAttribute` | ClassModes / Property 的 `Attributes` | |

行为不变；仅编译器警告引导迁移。可用 `#pragma warning disable CS0618` 临时压制，或逐步改写。

## 3. 新诊断（2.2–2.5）

| ID | 版本 | 含义 | 处理 |
|----|------|------|------|
| `Mccj009` | 2.2 | `InheritStr` 类型名非法 | 检查拼写 / `nameof`；优先改 `InheritType` |
| `Mccj010` | 2.2 | `Attributes` 字符串无法解析 | 去掉无效片段 |
| `Mccj011` | 2.4 | 接口成员未投影到生成类 | 增加带 `AutoCodeProperty` 的 mixin，或开 `InjectMarkerMembers` |
| `Mccj012` | 2.4 | Info：建议改用 CrudModes | Code Fix 可一键替换（2.5） |
| `Mccj013` | 2.4 | Index 列不在 Entity | 补字段或改 Spec |
| `Mccj014` | 2.5 | Relation 缺 TargetName | 填写 `nameof` |

## 4. 可选：改用 `AutoCodeCrudModes`（2.2）

```csharp
[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(IEntity<long>)])]
```

合并规则：同一 `Id` 的手写 `AutoCodeClassModes` **覆盖**预设。`OmitModes`：`EntityOnly`、`NoUpdate` 等。

## 5. 可选：`AutoCodeRelation`（2.2） / `AutoCodeMorph`（2.3）

外键四段式 → `[AutoCodeRelation]`；文件/枚举/区间/CSV → `[AutoCodeMorph]`。

## 6. 可选：列 / 索引 / ModeFlags（2.4）

```csharp
[AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
[AutoCodeColumn(Length = 64)]
string? ProductId { get; set; }

[AutoCodeIndex("IX_CreateTime", Spec = "CreateTime:Desc")]
```

手写 Create/Update/Detail 可用 `InheritDto = true` 代替 `InheritStr = nameof(X)+"Dto"`。

审计：复制 [CONVENTIONS_TEMPLATE.md](CONVENTIONS_TEMPLATE.md) 中的 `IHasCreateTime`，或用 `InjectMarkerMembers = true`（2.5）。

## 7. 可选：2.5 捷径

```csharp
[AutoCodeCrudModes(
    EntityInterfaceTypes = [typeof(ICreateTimeEntity<long>)],
    InjectMarkerMembers = true,
    SearchRangeType = typeof(SearchRangeForStructValue<>))] // 或 SearchRangeTemplate = "…<{0}>"
```

```xml
<AutoModeSearchRangeTemplate>SearchRangeForStructValue&lt;{0}&gt;</AutoModeSearchRangeTemplate>
```

## 8. 建议节奏

| 阶段 | 动作 |
|------|------|
| 当天 | 升到 **1.0.1.x**，处理 Mccj011/014；对 Mccj012 / CS0618 用 Code Fix 或改写 |
| 本周 | 新实体用预设 + ModeFlags + Column/Index / InjectMarkerMembers |
| 后续 | 迁移 Relation / Morph；采用 Conventions 模板 |

主路径文档：[Analyzers README](../src/AutoModeCodeGenerator.Analyzers/README.md) · [AI_CONSUMER_GUIDE.md](AI_CONSUMER_GUIDE.md) · Samples `CrudCookbook`。
