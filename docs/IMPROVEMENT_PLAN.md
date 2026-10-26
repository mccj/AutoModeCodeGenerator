# 改进计划（历史归档）

> **本文为阶段 A–I 的完成记录，不再扩展新阶段。**  
> 正式包为 **1.0.1.x**。后续见 **[ROADMAP.md](ROADMAP.md)**；DX 细则见 **[ANALYZER_DX_IMPROVEMENTS.md](ANALYZER_DX_IMPROVEMENTS.md)**；升级见 [UPGRADE.md](UPGRADE.md)。

依据五个生产项目的用法调研（内部笔记，未入库）。原规划路径（已完成）：兼容 → CRUD 预设与关系 → Morph / Column / DX。文中部分早期草案命名未采用，以最终 API 为准。

> **状态：** 阶段 A–I 已落地，随 **1.0.1.0** 发包（含 Obsolete / 双主入口）。

```mermaid
flowchart LR
  docs[A 文档与Samples] --> r1[B 兼容与诊断]
  r1 --> r2[C CRUD预设]
  r2 --> r3[D 关系助手]
  r3 --> r4[E Morph与特性模板]
  r4 --> r5[F Conventions包]
  r5 --> r6[H 契约诊断与列索引]
  r6 --> r7[I DX与注入]
```

---

## 阶段 A — 文档与示例对齐（立即，无破坏） ✅

**目标：** 让新用户按生产主路径上手；避免误以为 Record/Inpc/entity.json 是主用法。

| 任务 | 说明 | 产出 |
|------|------|------|
| A1 | 消费方 README 增加「推荐：CRUD 六模式」教程 | Analyzers README |
| A2 | Samples 增加迷你 CRUD 实体（含 FK 四段式、Search `ToNullable`、mixin） | Samples |
| A3 | 根 README / CHANGELOG 链接本分析与计划 | README、CHANGELOG |
| A4 | 标明扩展能力（Record / Inpc / entity.json）与主路径的优先级 | Analyzers README |

**完成标准：** 按文档可复现与生产一致的 Entity/Dto/Create/Update/Detail/Search 结构。

---

## 阶段 B — 兼容增强与诊断（短周期） ✅（并入 1.0.1）

**目标：** 不改变 API 形状，降低现有 1.0.0.x 配方升级到 1.0.1.x 的摩擦。

| ID | 任务 | 优先级 | 说明 |
|----|------|--------|------|
| B1 | 回归：1.0.0.x CRUD 属性集在 1.0.1 上的兼容测试 | P0 | InheritStr、ToNullable、InterfaceTypes、Attributes、PropertyTypeStr、Suffix、多 Ids |
| B2 | 诊断：`InheritStr` 目标类型名无效时 Warning | P1 | 减少拼写错误 |
| B3 | 诊断：建议对「仅有 Property 无 ClassModes」等已有规则补充文档示例 | P2 | 对接 Mccj004–008 |
| B4 | `Attributes` 解析失败时给出更明确的 Mccj 诊断（若当前吞异常） | P1 | |
| B5 | 升级指南：1.0.0.9–1.0.0.17 → 1.0.1.x | P0 | 文档：先升级包验证，再采用预设 |

**完成标准：** 用生产风格接口规格写一组集成测试；升级指南可执行。

---

## 阶段 C — CRUD 预设（核心价值） ✅

**目标：** 消灭每实体 6 行 `AutoCodeClassModes` 样板。

### 建议 API（草案）

```csharp
[AutoCodeCrudModes]  // 或 AutoCodeClassModesPreset
public interface Product : BaseEntity<long>
{
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search])]
    string Name { get; set; }
}
```

展开为等价的现有六模式（Id/Suffix/`InheritStr`/`ToNullable`/`InterfaceTypes` 可覆盖）：

| 可选项 | 默认 |
|--------|------|
| `EntitySuffix` | `Record`（内控等项目需可改为 `Entity`） |
| `DtoSuffix` | `Dto` |
| `KeyType` / `EntityInterfaces` | `IEntity<{TKey}>` |
| `OmitModes` | 无（支持 EntityOnly、NoUpdate 等） |
| `SearchToNullable` | `true` |
| `CreateUpdateDetailInheritDto` | `true` |

**实现要点：**

- Preset **生成**与现有 `AutoCodeClassModes` 相同的内部模型，或在 Extract 阶段展开，避免双轨发射器。
- 允许与手写 `AutoCodeClassModes` 并存（手写优先或合并策略需明确并测）。
- 常量类 `AutoCodeCrud`（Entity/Dto/Create/Update/Detail/Search）随 PostInitialization 注入，替代各项目自建 `AutoGeneratorConst` 的 Id 部分。

| ID | 任务 | 优先级 |
|----|------|--------|
| C1 | 设计 `AutoCodeCrudModes` + `AutoCodeCrud` 常量并写规格 | P0 |
| C2 | Extract 阶段展开预设；单元测试对齐现有六模式输出 | P0 |
| C3 | `OmitModes` / Entity-only / 自定义 Suffix | P1 |
| C4 | 文档 + Samples 迁移示例（旧 6 行 → 1 行） | P0 |
| C5 | （可选）分析器建议：检测到 6 行标准模式时提示改用预设 | P2 |

**完成标准：** Samples 与至少一套「仿生产」测试仅用预设即可生成完整 CRUD 类型。

---

## 阶段 D — 关系助手 ✅（主干已并入 1.0.1）

**目标：** 压缩外键四段式。

### 建议 API（草案）

```csharp
[AutoCodeRelation(
    TargetName = nameof(Product),  // 或 TargetModeSuffix 约定
    IncludeNavigate = true,
    NavigateTemplate = "SqlSugarManyToOne")]
public long Product { get; set; }
```

展开为现有：`Suffix=Id`、Detail 类型、Search 类型、Navigate `Attributes`。

| ID | 任务 | 优先级 |
|----|------|--------|
| D1 | 规格：mode 映射表、命名约定、可空规则 | P0 |
| D2 | 实现 `AutoCodeRelation` → 多条 PropertyInfo | P0 |
| D3 | 可插拔 Navigate 模板（SqlSugar 默认；文档说明 EF 自定义） | P1 |
| D4 | 与预设、手写 Property 共存与冲突诊断 | P1 |
| D5 | 文档：从四段式迁移到 Relation | P0 |

**完成标准：** Device/供暖中的典型 FK 可用单特性表达且生成结果一致。

---

## 阶段 E — Morph 与特性模板 ✅

**目标：** 覆盖文件 / 枚举 / 区间 / CSV 等双轨字段。

| ID | 任务 | 优先级 |
|----|------|--------|
| E1 | `AutoCodeMorph.File` / `SearchRange` / `StringEnum` / `StringArray` 等 | P1 |
| E2 | 按 mode 绑定不同 Attribute 宏（Entity vs Api） | P1 |
| E3 | 文档化宏列表；允许项目级字符串覆盖 | P2 |

**完成标准：** Payment 文件字段、枚举双 Attribute、SearchRange 可用声明式表达。

---

## 阶段 F — Conventions 包（可选，并行） ✅

**目标：** 减少五项目复制 `AutoGeneratorConst` / 审计 mixin。

| ID | 任务 | 优先级 |
|----|------|--------|
| F1 | 文档模板：推荐 Const + BaseEntity 源码（可复制） | P1 |
| F2 | （可选）`AutoModeCodeGenerator.Conventions.SqlSugar` 源码/包 | P2 |
| F3 | 不强制依赖 SqlSugar；主包保持 ORM 无关 | P0（约束） |

---

## 阶段 H — 契约诊断与 DX（基于 DeviceServer inbox 迁移） ✅

| ID | 任务 | 状态 |
|----|------|------|
| H1 | `Mccj011`：InterfaceTypes 成员未生成属性 | ✅ |
| H2 | `Mccj012`：手写六模式建议 CrudModes | ✅ |
| H3 | `AutoCodeColumn` / `AutoCodeIndex` / `ModeFlags` / `InheritDto` | ✅ |
| H4 | `Mccj013`：索引列校验 | ✅ |
| H5 | Conventions：CreateTime mixin、长度/索引模板、Entity-only 示例 | ✅ |
| H6 | 文档：AI 指南、升级说明、Conventions 模板、Analyzers README | ✅ |

## 阶段 I — DX 加强 ✅

| ID | 任务 | 状态 |
|----|------|------|
| I1 | Detail/Dto 同类型去重（消 CS0108） | ✅ |
| I2 | `InjectMarkerMembers` + SearchRange 模板（特性 / MSBuild） | ✅ |
| I3 | `Mccj014` Relation 空 TargetName | ✅ |
| I4 | Mccj012 DiagnosticAnalyzer + CodeFix | ✅ |
| I5 | DateTime Converter 宏（AttrMacros + Conventions） | ✅ |

## 阶段 G — 明确不做或降级

| 项 | 决策 |
|----|------|
| 默认 ClassStyle 改为 Record | **不做**（破坏全部 CRUD） |
| Inpc 作为主推 | **降级**为扩展文档 |
| entity.json 多 mode 深化 | **维持**现状，文档标明次要 |
| 要求消费方立刻改用 CrudModes | **分阶段**：先兼容证明，再推预设 |

**阶段 I 之后的非目标与可选工作**已迁至 [ROADMAP.md](ROADMAP.md)；Analyzers DX 细则见 [ANALYZER_DX_IMPROVEMENTS.md](ANALYZER_DX_IMPROVEMENTS.md)，避免本计划无限加阶段。

---

## 里程碑与建议排期

| 里程碑 | 内容 | 建议 |
|--------|------|------|
| M0 | 阶段 A 文档/Samples | ✅ |
| M1 | 阶段 B 兼容与诊断 | ✅ 并入 **1.0.1.0** |
| M2 | 阶段 C + D 主干 | ✅ **1.0.1.0** |
| M3 | 阶段 E + F | ✅ **1.0.1.0** |
| M4 | 阶段 H | ✅ **1.0.1.0** |
| M5 | 阶段 I | ✅ **1.0.1.0** |

---

## 风险与约束

1. **向后兼容：** 预设必须能生成与手写六模式等价的类型名/继承，否则无法替换生产代码。
2. **ORM 无关：** Navigate/Column 宏可默认 SqlSugar 字符串，但通过模板配置，避免主包硬依赖。
3. **版本沟通：** 消费方钉死旧版；发布说明需强调「CRUD 主路径兼容 + 可选预设」。
4. **测试策略：** 增加 `CrudPresetParityTests`：同一规格手写六模式 vs 预设，断言生成文本或编译符号一致。

---

## 成功指标

- 新实体规格：ClassModes 样板行数从 ~6 降至 **0–1**（预设）。
- 典型 FK：Property 特性条数从 ~4 降至 **1**（Relation）。
- 五个参考项目可在不改业务语义的前提下升级到含预设的版本（允许逐步替换）。
- 文档首页教程与 Samples 反映真实主路径；Record/Inpc/entity.json 降为「高级/扩展」。
