# Samples

本地验证 `AutoModeCodeGenerator.Analyzers`（`net8.0`，`ProjectReference` 引用分析器）。对应包版本 **1.0.1+**。

## 文件

| 文件 | 说明 |
|------|------|
| [`CrudCookbook/CrudProduct.cs`](CrudCookbook/CrudProduct.cs) | **入口 A：** `AutoCodeCrudModes` + Relation + Morph |
| [`CrudCookbook/InboundRawV24.cs`](CrudCookbook/InboundRawV24.cs) | ModeFlags、Column、Inject、Entity-only |
| [`TestModel.entity.json`](TestModel.entity.json) | **入口 B：** `.entity.json` 声明式生成 |
| [`TestModel.interface.cs`](TestModel.interface.cs) | ClassStyle Record / Inpc、接口继承（含 Obsolete ClassModes 样例） |
| [`Program.cs`](Program.cs) | 引用生成类型并打印 |

## 运行

```bash
dotnet run --project Samples.csproj
```

生成代码默认落盘到 `obj/Generated`（见 csproj 中 `EmitCompilerGeneratedFiles`）。

## 文档

- [Analyzers README](../AutoModeCodeGenerator.Analyzers/README.md)
- [AI 指南](../../docs/AI_CONSUMER_GUIDE.md)
- [升级指南](../../docs/UPGRADE_1x_TO_2x.md)
- [Conventions 模板](../../docs/CONVENTIONS_TEMPLATE.md)
- [路线图](../../docs/ROADMAP.md)
