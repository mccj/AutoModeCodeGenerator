# 文档索引

当前稳定版本：**1.0.1.0**。

## 按角色

| 你是… | 先读 |
|--------|------|
| 人类开发者（接包） | [Analyzers README](../src/AutoModeCodeGenerator.Analyzers/README.md) → [UPGRADE](UPGRADE_1x_TO_2x.md) → [CONVENTIONS_TEMPLATE](CONVENTIONS_TEMPLATE.md) |
| AI / Agent | [AI_CONSUMER_GUIDE.md](AI_CONSUMER_GUIDE.md)（**§0 目的/架构/优势** → 决策树强制） |
| 维护者 / 规划 | [ARCHITECTURE.md](ARCHITECTURE.md) · [ROADMAP.md](ROADMAP.md) · [ANALYZER_DX_IMPROVEMENTS.md](ANALYZER_DX_IMPROVEMENTS.md) · [IMPROVEMENT_PLAN.md](IMPROVEMENT_PLAN.md) |
| 发包 | [PACKAGING.md](../src/AutoModeCodeGenerator.Analyzers/PACKAGING.md) · [CHANGELOG.md](../CHANGELOG.md) |

## 按主题

| 文档 | 说明 |
|------|------|
| [AI_CONSUMER_GUIDE.md](AI_CONSUMER_GUIDE.md) | **目的 / 消费架构 / 优势**、决策树、CRUD/Relation/Morph、禁忌、Mccj 速查 |
| [UPGRADE_1x_TO_2x.md](UPGRADE_1x_TO_2x.md) | 1.x → 2.6：升包、Obsolete、诊断、预设迁移 |
| [CONVENTIONS_TEMPLATE.md](CONVENTIONS_TEMPLATE.md) | Id 常量、CreateTime mixin、列/索引、Entity-only、DateTime 宏 |
| [ARCHITECTURE.md](ARCHITECTURE.md) | 双主入口、生成器管线、分层、改代码约定 |
| [ROADMAP.md](ROADMAP.md) | **2.6 之后**：建议下一步与明确非目标 |
| [ANALYZER_DX_IMPROVEMENTS.md](ANALYZER_DX_IMPROVEMENTS.md) | DeviceServer 试点驱动的 Analyzers DX 细则 |
| [IMPROVEMENT_PLAN.md](IMPROVEMENT_PLAN.md) | 阶段 A–I 完成记录（历史，勿再开新阶段） |

## 版本能力速查

| 版本 | 关键词 |
|------|--------|
| 2.2 | `AutoCodeCrudModes`、`AutoCodeRelation`、Mccj009/010 |
| 2.3 | `AutoCodeMorph`、AttrMacros、Conventions.SqlSugar |
| 2.4 | `AutoCodeColumn`、`AutoCodeIndex`、`ModeFlags`、`InheritDto`、Mccj011–013 |
| 2.5 | Detail 去重、`InjectMarkerMembers`、`AutoModeSearchRangeTemplate`、Mccj014、Mccj012 CodeFix |
| 2.5.1 | `SearchRangeType` / Morph `RangeType` |
| 1.0.1.0（nuget） | ClassModes/`*Str` Obsolete；双主入口；含原 2.2–2.6 能力（2.x 未单独发包） |

消费方包 README（随 NuGet）：[../src/AutoModeCodeGenerator.Analyzers/README.md](../src/AutoModeCodeGenerator.Analyzers/README.md)。
