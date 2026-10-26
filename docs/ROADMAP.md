# 路线图与非目标（1.0.1 之后）

> nuget.org 正式包：**1.0.1.x**（AppVeyor `1.0.1.{build}`）。自 **1.0.0.17** 起累计：CRUD 预设、Relation、Morph、Column/Index、Inject、诊断/CodeFix、Obsolete 软弃用、双主入口。  
> 本文记录**仍值得做**与**明确不做**，避免继续堆 API。  
> **Analyzers DX 细则**（DeviceServer 试点驱动）：[ANALYZER_DX_IMPROVEMENTS.md](ANALYZER_DX_IMPROVEMENTS.md)。

## 当前状态

| 包版本 | 主题 |
|--------|------|
| **1.0.0.17**（nuget 旧） | 手写 ClassModes / Property 主路径 |
| **1.0.1.0+**（nuget 新） | CrudModes / Relation / Morph / Column / Index / Inject / Mccj009–014 / Obsolete / 双主入口 |

消费方若仍钉在 **1.0.0.9–1.0.0.17**，优先升到 **1.0.1.x**。见 [UPGRADE.md](UPGRADE.md)。

## 建议下一步（收益递减区）

按优先级（细则与验收见 [ANALYZER_DX_IMPROVEMENTS.md](ANALYZER_DX_IMPROVEMENTS.md)）：

1. **消费方升级与迁移**（最高优先） — 升 **1.0.1.x** 时处理 CS0618 / Mccj012。

2. **Mccj012 语义化 + CodeFix 保配置**（Analyzers P0）  
   支持 `Id = C.EntityId` 等常量；语义匹配属性类型；一键替换保留 SugarTable / Attributes / Namespace。

3. **SuggestRelation / RelationNameMismatch / SearchRange Morph 建议**（Analyzers P1）

4. **约定卫生检查**（Analyzers P2）

5. **`InjectMarkerMembers` 白名单**（若继续改生成器）

6. **Relation 一对多 / 反向 / FK 重命名选项**

7. **黄金文件 / 快照测试**

8. **entity.json 与 Expand 对齐**（可选）— 若入口 B 需要 CrudModes/Relation 语义，再评估，勿 silently 改默认。

## 明确不做（短期）

| 项 | 原因 |
|----|------|
| 删除 ClassModes / `*Str`（`error: true`） | 1.0.1 仅软弃用；行为保留至有明确大版本计划 |
| 默认 `ClassStyle.Record` / 主推 Inpc | 会破坏 CRUD POCO 假设 |
| 主包引用 SqlSugar.dll | 保持 ORM 无关；字符串模板足够 |
| 生成 AppService / Controller | 超出「规格 → 多产物类型」边界 |
| 强制 Morph.StringEnum 为唯一枚举路径 | DeviceServer 验证：保留 CLR 枚举 + 双特性更合适 |
| 强制重命名 FK / File / M2M 必须用 Relation/Morph | 允许手写例外；分析器勿误报 |

## 文档入口

| 角色 | 文档 |
|------|------|
| 人类消费方 | [Analyzers README](../src/AutoModeCodeGenerator.Analyzers/README.md) |
| AI / Agent | [AI_CONSUMER_GUIDE.md](AI_CONSUMER_GUIDE.md) |
| 升级 | [UPGRADE.md](UPGRADE.md) |
| 约定复制 | [CONVENTIONS_TEMPLATE.md](CONVENTIONS_TEMPLATE.md) |
| 维护者架构 | [ARCHITECTURE.md](ARCHITECTURE.md) |
| **Analyzers DX 细则** | [ANALYZER_DX_IMPROVEMENTS.md](ANALYZER_DX_IMPROVEMENTS.md) |
| 历史计划 | [IMPROVEMENT_PLAN.md](IMPROVEMENT_PLAN.md) |
