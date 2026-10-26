# AutoModeCodeGenerator

[![Build status](https://ci.appveyor.com/api/projects/status/yq70walphj0ewuob?svg=true)](https://ci.appveyor.com/project/mccj/automodecodegenerator)
[![MyGet](https://img.shields.io/myget/mccj/vpre/AutoModeCodeGenerator.Analyzers.svg)](https://myget.org/feed/mccj/package/nuget/AutoModeCodeGenerator.Analyzers)
[![NuGet](https://img.shields.io/nuget/v/AutoModeCodeGenerator.Analyzers.svg)](https://www.nuget.org/packages/AutoModeCodeGenerator.Analyzers)
[![Apache License 2.0](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE)

基于 Roslyn 的 C# **增量源生成器**：在接口/类上标注特性，编译期生成 SqlSugar / Admin.NET 风格的 **Entity / Dto / Create / Update / Detail / Search** 等多产物类型（只生成数据形状，不生成 Service / Controller）。

**当前版本：1.0.1.0**

**为何用：** 一份接口规格驱动六套 CRUD 类型，按 mode 投影字段，mixin 复用横切；用预设 / Relation / Morph 消灭六模式与外键样板。亦支持 `.entity.json` 声明式生成。消费架构与 Agent 硬规则见 [docs/AI_CONSUMER_GUIDE.md](docs/AI_CONSUMER_GUIDE.md)（§0）。

## 功能概览

相对 nuget.org **1.0.0.17**，自 **1.0.1.0** 起一并提供：

| 能力 | 说明 |
|------|------|
| `[AutoCodeCrudModes]` | 一行展开 CRUD 六模式；`OmitModes` 支持 Entity-only |
| `[AutoCodeRelation]` | 外键 → Id / Detail / Search / Navigate |
| `[AutoCodeMorph]` | 文件 / 枚举双轨 / Search 区间 / CSV↔数组 |
| `[AutoCodeColumn]` / `[AutoCodeIndex]` | 列长 / BigString / SugarIndex |
| `ModeFlags` / `InheritDto` | 压缩 Ids；免拼 `InheritStr` |
| `InjectMarkerMembers` | 自动投影 `InterfaceTypes` 未声明成员（如 CreateTime） |
| Detail↔Dto 去重 | 继承同类型时避免 CS0108 |
| 诊断 Mccj001–014 | 含 Mccj012 CodeFix「改用 AutoCodeCrudModes」 |
| `SearchRangeType` / Morph `RangeType` | 区间开泛型 `typeof(...)`（亦可用字符串模板） |
| Obsolete 软弃用 | ClassModes / `*Str` / CustomAttribute → 新 API（行为保留） |

**双主入口：** 特性（CRUD 预设等）与 `.entity.json` 并列；默认 ClassStyle **Poco**。`ClassStyle.Record` / `Inpc` 为样式选项。

## 文档导航

| 文档 | 对象 | 内容 |
|------|------|------|
| [Analyzers/README.md](src/AutoModeCodeGenerator.Analyzers/README.md) | 消费方 | **主文档**：安装、CRUD、Morph、Column/Index、诊断、MSBuild |
| [docs/AI_CONSUMER_GUIDE.md](docs/AI_CONSUMER_GUIDE.md) | **AI / Agent** | 目的/架构/优势、决策树、标准模板、禁忌、诊断速查 |
| [docs/UPGRADE.md](docs/UPGRADE.md) | 消费方 | 1.0.0.x → 1.0.1.x 升级与 Obsolete 迁移 |
| [docs/CONVENTIONS_TEMPLATE.md](docs/CONVENTIONS_TEMPLATE.md) | 消费方 | mixin / CreateTime / 列索引可复制模板 |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | 维护者 | 双主入口、管线与分层 |
| [docs/ROADMAP.md](docs/ROADMAP.md) | 维护者 | **1.0.1 之后**建议与非目标 |
| [docs/ANALYZER_DX_IMPROVEMENTS.md](docs/ANALYZER_DX_IMPROVEMENTS.md) | 维护者 | DeviceServer 试点驱动的 Analyzers DX 细则 |
| [docs/IMPROVEMENT_PLAN.md](docs/IMPROVEMENT_PLAN.md) | 维护者 | 阶段 A–I 完成记录（历史） |
| [CHANGELOG.md](CHANGELOG.md) | 所有人 | 版本变更 |
| [PACKAGING.md](src/AutoModeCodeGenerator.Analyzers/PACKAGING.md) | 维护者 | NuGet / MyGet 发包 |
| [Samples](src/AutoModeCodeGenerator.Samples/) | 本地验证 | CrudCookbook 等 |

更多索引：[docs/README.md](docs/README.md)。

## 安装

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.1.0" PrivateAssets="all" />
```

可选约定包（无 SqlSugar 程序集依赖）：

```xml
<PackageReference Include="AutoModeCodeGenerator.Conventions.SqlSugar" Version="1.0.1.0" />
```

完整说明见 **[Analyzers/README.md](src/AutoModeCodeGenerator.Analyzers/README.md)**（随 NuGet 发布）。

### 本地 ProjectReference

```xml
<ProjectReference Include="..\AutoModeCodeGenerator.Analyzers\AutoModeCodeGenerator.Analyzers.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

### 常用 MSBuild

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
  <AutoModeGenerateEntityFiles>false</AutoModeGenerateEntityFiles>
  <!-- Morph / InjectMarkerMembers 默认区间类型；Device 项目可改为 SearchRangeForStructValue<{0}> -->
  <AutoModeSearchRangeTemplate>SearchRangeForStructValue&lt;{0}&gt;</AutoModeSearchRangeTemplate>
</PropertyGroup>
```

## 最小示例

```csharp
using AutoCodeGenerator;

[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(IEntityMarker)])]
public interface Product : IHasLongId
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    [AutoCodeColumn(Length = 128)]
    string? Name { get; set; }

    [AutoCodeRelation(TargetName = nameof(Category), IncludeNavigate = false)]
    long Category { get; set; }
}
```

生成：`ProductRecord`、`ProductDto`、`ProductCreateInput`、`ProductUpdateInput`、`ProductDetailOutput`、`ProductSearchInput`。

## 项目结构

```
AutoModeCodeGenerator/
├── README.md / CHANGELOG.md / LICENSE
├── docs/                          # 消费方/AI/维护者文档（见 docs/README.md）
└── src/
    ├── AutoModeCodeGenerator.Analyzers/      # 源生成器 + 诊断/CodeFix（NuGet）
    │   ├── Extraction/ / Expansion/          # 管线：见 docs/ARCHITECTURE.md
    │   ├── CodeGeneration/ / EntityFiles/    # 共享 Emit / entity.json
    │   ├── Diagnostics/ / Templates/         # Mccj* / 嵌入公共 API
    ├── AutoModeCodeGenerator.Conventions.SqlSugar/
    ├── AutoModeCodeGenerator.Samples/
    └── AutoModeCodeGenerator.Tests/
```

| 项目 | TFM | 说明 |
|------|-----|------|
| Analyzers | netstandard2.0 | **1.0.1.0**，`PrivateAssets=all` 消费 |
| Conventions.SqlSugar | netstandard2.0 | 可选字符串模板 |
| Samples | net8.0 | 本地验证 |
| Tests | net8.0 | GeneratorDriver + Analyzer/CodeFix |

公共 API 命名空间：`AutoCodeGenerator`。内部：`AutoModeCodeGenerator.Internal`。

## 开发与构建

```bash
dotnet build src/AutoModeCodeGenerator.slnx
dotnet test src/AutoModeCodeGenerator.Tests/AutoModeCodeGenerator.Tests.csproj
dotnet run --project src/AutoModeCodeGenerator.Samples/Samples.csproj
dotnet pack src/AutoModeCodeGenerator.Analyzers/AutoModeCodeGenerator.Analyzers.csproj -c Release
```

- 测试覆盖：Poco/Record/Inpc、CRUD/Relation/Morph、Column/Index、Inject、Mccj001–014、CodeFix、nupkg 内容。
- CI：[`.github/workflows/ci.yml`](.github/workflows/ci.yml)；发包见 [PACKAGING.md](src/AutoModeCodeGenerator.Analyzers/PACKAGING.md)。

### 调试源生成器

在 `AutoCodeGenerator.Initialize` 开头临时 `Debugger.Launch();`，编译消费项目时附加；调试完移除。

## 1.0.1 破坏性变更（相对 1.0.0.17）

- `ClassStyle.Record` → 真正的 `record class`（`init`）
- Inpc 基类实现 `INotifyPropertyChanged`
- 内部命名空间改为 `AutoModeCodeGenerator.Internal`

详见 [CHANGELOG.md](CHANGELOG.md)。CRUD 默认 **Poco** 时升到 1.0.1.x **无破坏**。

## 许可证

[Apache License 2.0](LICENSE)
