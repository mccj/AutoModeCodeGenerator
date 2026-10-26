# Analyzers DX 改进建议（基于 DeviceServer 试点）

> **状态：** 规划草案（2026-10-26）；**2.5.1** 已落地 `SearchRangeType` / Morph `RangeType`（字符串模板仍兼容）。  
> **依据：** `物联网设备平台-DeviceDriver5` 升至 **2.5.x** 的真实用法（见消费方 `EntityAutoGenerator/PILOT_2x.md`）  
> **定位：** 补充 [ROADMAP.md](ROADMAP.md) 的细化清单；**不以再堆 Morph/Relation API 为主**，优先补齐 IDE 诊断与 CodeFix，消灭一次性迁移脚本。  
> **历史计划：** 阶段 A–I 见 [IMPROVEMENT_PLAN.md](IMPROVEMENT_PLAN.md)（已归档，勿再开新阶段）。

## 背景结论

2.5.0 的**生成能力**已覆盖 DeviceServer 主路径（`AutoCodeCrudModes`、简单 `AutoCodeRelation`、`Morph.SearchRange` + `AutoModeSearchRangeTemplate`）。

真正薄弱的是 **Roslyn DiagnosticAnalyzer / CodeFix 面**：

| 现状 | 说明 |
|------|------|
| 生成器诊断 Mccj001–014 | 管线内校验较全（mode 绑定、接口契约、索引列、空 TargetName 等） |
| 真正的 `DiagnosticAnalyzer` | **仅** `SuggestCrudModesAnalyzer` → Mccj012 |
| CodeFix | **仅**「六行 ClassModes → CrudModes」，且易丢配置 |
| 试点迁移手段 | `_migrate_crud_modes.py` / `_migrate_relation_morph.py` 等一次性脚本 |

**核心判断：** 少加 API、多加能命中生产写法（`Id = C.EntityId`、真实 FK/Morph）的 IDE 规则，把脚本变成可持续诊断。

### 试点 house style（分析器应尊重，勿强推单一路径）

| 项 | DeviceServer 选择 |
|----|-------------------|
| 类头 | 一律 `AutoCodeCrudModes` |
| 简单 FK（属性名 ≈ 目标名） | `AutoCodeRelation` |
| 重命名 FK（如 `FirmwareConfig` → `Configs`） | **仍手写**四段式 |
| Search 区间 | 优先 `Morph.SearchRange` + MSBuild 模板；部分旧字段仍显式 `PropertyType` |
| 枚举 | **不用** `Morph.StringEnum`；Entity / Dto+Search **双 `AutoCodeProperty`** |
| 文件字段 | 仍手写 `FileBase64*` |
| M2M Navigate | 仍手写 |

---

## 建议落地顺序（约 2.6 量级）

```mermaid
flowchart LR
  p1[1 Mccj012 语义化与 CodeFix] --> p2[2 SuggestRelation]
  p2 --> p3[3 SearchRange / 冗余模板]
  p3 --> p4[4 约定卫生检查]
  p4 --> p5[5 Inject 白名单与 Relation API]
```

| 优先级 | 主题 | 类型 | 预期收益 |
|--------|------|------|----------|
| **P0** | Mccj012 语义化 + CodeFix 保配置 | Analyzer / Fix | 其余 1.x 项目可真正一键迁 |
| **P1** | SuggestRelation + RelationNameMismatch | Analyzer / Fix | 消灭下一批 FK 迁移脚本 |
| **P1** | SearchRange Morph 建议 + 冗余 Template | Analyzer | 对齐 MSBuild 模板、去样板漂移 |
| **P2** | Enum 双轨完整性、EntityOnly 卫生 | Analyzer | 守住 house style |
| **P2** | Inject 白名单、Relation FK 重命名 API | Generator + 诊断 | 第二消费者有需求再做 |
| **P2** | 黄金快照测试 | Tests | 防 Morph/Relation 回归 |

---

## P0 — Mccj012 / CodeFix 修通生产路径

### 问题

1. **Id 只认字符串字面量**  
   `TryGetIdArgument` 不解析 `Id = C.EntityId` / `AutoCodeCrud.Entity` → 真实 1.x 规格**不触发** Mccj012。
2. **属性名字符串匹配**  
   `name.Contains("AutoCodeClassModes")`，未用语义 `GetTypeInfo` / 属性类型全名。
3. **CodeFix 丢配置**（ROADMAP 已记）  
   仅从 Entity 模式保留部分 `InterfaceTypes` / `Attributes`；丢失 `SugarTable`、自定义 `Namespace` / Suffix / `ToNullable` 等。

### 建议

| ID | 任务 | 验收 |
|----|------|------|
| A1 | Id 支持常量字段、`nameof`、以及与 `AutoCodeModeIds` 等价的项目常量（如 `AutoGeneratorConst.EntityId`） | 仿生产规格触发 Mccj012 |
| A2 | 属性识别改为语义匹配（含别名 using） | 重命名/全局别名不漏报 |
| A3 | CodeFix 保留 Entity 上 `Attributes`、`Namespace`、以及可映射到 `AutoCodeCrudModes` 的选项 | 一键替换后编译语义不变 |
| A4 | 单测：字面量 Id **与** `C.EntityId` 两套规格 | `SuggestCrudModes*` 测试覆盖 |

---

## P1 — Relation / Morph IDE 规则

### SuggestRelation（建议诊断 + CodeFix）

**触发：** 同一属性上典型四段 FK（Entity+`Suffix=Id`、Detail、Search、Navigate），且属性名与导航目标名一致（或可推断）。

**动作：** Info + CodeFix → `[AutoCodeRelation(TargetName = nameof(...))]`。

**勿触发：** 属性名 ≠ 目标类型名（如 `FirmwareConfig` → `Configs`）、含自定义 `Prefix="Is"` 且无法由现有 Relation API 表达、M2M / 文件字段。

### RelationNameMismatch

**触发：** 已写 `AutoCodeRelation`，但 `TargetName` 与属性名不一致，且未提供「外键/导航名」覆盖选项。

**动作：** Warning — Navigate 可能生成错误 `nameof(Target)Id`；应手写四段式或等待 Relation API 扩展。

### SuggestSearchRangeMorph

**触发：** Entity+Dto 上显式 `PropertyType = …SearchRange…`，且工程已配置 `AutoModeSearchRangeTemplate`（或类上已有相同模板）。

**动作：** Info → 建议改为 `AutoCodeMorph(Kind = SearchRange)`。

### RedundantSearchRangeTemplate / 无用 CrudModes 选项

**触发（任一）：**

- 类上 `SearchRangeTemplate` 与 MSBuild `AutoModeSearchRangeTemplate` **字面量相同**；
- `OmitModes = EntityOnly` 仍带 Search/Create/Update/Detail 的 SummaryPrefix 或 SearchRangeTemplate（迁移脚本遗留）。

**动作：** Info — 可删冗余参数。

### Relation + 手写四段并存

在最终 Mccj006（重名跳过）之前，尽早 Warning「同一逻辑 FK 重复投影」。

---

## P2 — 约定卫生与可选生成器增强

### Enum 双轨完整性

枚举属性须满足其一：

- `Morph.StringEnum`，或  
- Entity 侧字符串枚举特性 + Dto/Search 侧 OpenAPI 枚举特性（双 `AutoCodeProperty`）。

缺一边 → Warning。**不要**把 Morph.StringEnum 设成唯一合法路径（DeviceServer 已验证默认 Morph 不合适）。

### EntityOnly + 基类搭配

`OmitModes = EntityOnly` 却继承会把成员投到 Update/Detail 的 `BaseEntity` → 易 Mccj005；提示改用 `BaseEntityOnlyId`（或项目等价 mixin）。

### MSBuild 与 Morph 一致性

存在 `Morph.SearchRange`，但既无类级 `SearchRangeTemplate`、也无有效 `AutoModeSearchRangeTemplate` → Warning（避免 silent 用错区间类型）。

### InjectMarkerMembers 白名单（生成器）

只注入名称匹配白名单的成员（如 `Id` / `CreateTime`），避免 marker 接口过投影。对照：ThingModel 用 `ICreateTimeEntity.CreateTime`，基类用 `CreatedTime`。

### Relation API 小扩展（有第二消费者再做）

| 选项（草案） | 用途 |
|--------------|------|
| `ForeignKeyName` / `NavigatePropertyName` | `FirmwareConfig` → `Configs`Id |
| `IncludeSearchPresence`（Is\*） | Device 有、ProductConfig 无时的显式开关 |

无 API 时，分析器至少应 **禁止**「属性名 ≠ TargetName 的裸 Relation」。

### Morph 与自定义 ClassMode Id

Morph / Relation 硬编码 `AutoCodeModeIds`；自定义 mode Id 时可能静默不对齐。建议：无法绑定到已声明 mode 时 Warning（可与 Mccj005 协同，文案指向 Morph/Relation）。

### 黄金快照测试

对 `CrudCookbook`、inbox 风格样例做生成文本快照，防止再改 Morph/Relation/Inject 时回归试点配方。

---

## 建议的诊断 ID 预留（未正式分配）

> 正式编号前写入 `AnalyzerReleases.Unshipped.md`；下列仅为规划标签。

| 标签 | 大致含义 | 建议级别 |
|------|----------|----------|
| SuggestRelation | 四段 FK → Relation | Info + CodeFix |
| RelationNameMismatch | TargetName 与属性名不一致 | Warning |
| SuggestSearchRangeMorph | 显式 SearchRange → Morph | Info |
| RedundantCrudModesOption | 模板/Summary 与 MSBuild 重复或 EntityOnly 无用 | Info |
| EnumAttrIncomplete | 枚举双轨缺一侧 | Warning |
| EntityOnlyBaseMismatch | EntityOnly + 错误基类 | Warning |
| SearchRangeTemplateMissing | Morph.SearchRange 无模板来源 | Warning |
| DuplicateFkProjection | Relation 与手写四段并存 | Warning |

---

## 明确不做（与 ROADMAP 对齐）

| 项 | 原因 |
|----|------|
| 深化 `.entity.json` | 生产主路径未用 |
| 强制弃用六行 ClassModes | 兼容保留；仅 Info + CodeFix |
| 强制唯一枚举路径为 Morph.StringEnum | 与 DeviceServer house style 冲突 |
| 强制 Config 重命名 FK / File / M2M 必须 Relation/Morph | 允许手写「例外岛」 |
| 生成 AppService / Controller | 超出规格→多产物边界 |
| 主包引用 SqlSugar.dll | ORM 无关；字符串模板足够 |

---

## 与现有文档关系

| 文档 | 关系 |
|------|------|
| [ROADMAP.md](ROADMAP.md) | 高层优先级与非目标；本文为其 **Analyzers DX 细则** |
| [IMPROVEMENT_PLAN.md](IMPROVEMENT_PLAN.md) | 2.5 前阶段 A–I 归档 |
| [AI_CONSUMER_GUIDE.md](AI_CONSUMER_GUIDE.md) | 消费方 Agent 指南；落地诊断后需同步 Mccj 表与决策树 |
| [UPGRADE_1x_TO_2x.md](UPGRADE_1x_TO_2x.md) | 升级节奏；CodeFix 修好后可弱化「手写脚本迁移」描述 |
| Analyzers README | 消费方诊断表；新 ID 正式发布时同步 |

---

## 成功指标（2.6 建议）

1. 使用 `Id = C.EntityId` 的标准六模式规格，IDE 能报 Mccj012 且 CodeFix 后**无需手工补** SugarTable / Namespace。
2. 属性名与目标名一致的四段 FK，可一键改为 Relation；属性名 ≠ 目标名时有明确 Warning，而非 silent 错 Navigate。
3. 配置了 `AutoModeSearchRangeTemplate` 的工程中，类上重复模板与 EntityOnly 无用选项可被 Info 指出。
4. DeviceServer 类试点不再依赖仓库内 `_migrate_*.py` 做同类改造。
5. CrudCookbook / inbox 风格至少一组生成快照进入 CI。
