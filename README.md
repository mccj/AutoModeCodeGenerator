# AutoModeCodeGenerator

[![Build status](https://ci.appveyor.com/api/projects/status/yq70walphj0ewuob?svg=true)](https://ci.appveyor.com/project/mccj/automodecodegenerator)
[![MyGet](https://img.shields.io/myget/mccj/vpre/AutoModeCodeGenerator.Analyzers.svg)](https://myget.org/feed/mccj/package/nuget/AutoModeCodeGenerator.Analyzers)
[![NuGet](https://img.shields.io/nuget/v/AutoModeCodeGenerator.Analyzers.svg)](https://www.nuget.org/packages/AutoModeCodeGenerator.Analyzers)
[![Apache License 2.0](https://img.shields.io/badge/license-Apache%202.0-orange.svg)](LICENSE)

基于 Roslyn 的 C# 增量源生成器（Incremental Source Generator），通过特性标注在接口或类上，在编译期自动生成 POCO、Record、INPC 等样式的实现类。

## 功能概览

- 在**接口或类**上声明生成规则，按 `Id` 关联属性，一次定义可生成多个目标类
- 支持 **Poco**、**Record**、**Inpc**（属性变更通知）三种生成样式
- 自动继承接口成员、XML 文档注释、自定义 Attribute
- 可配置命名空间、类名前后缀、访问修饰符、Nullable 上下文等
- 作为 Analyzer 包发布，消费项目无需直接引用生成器程序集
- 使用 `IIncrementalGenerator` + `ForAttributeWithMetadataName`，编译性能更好

## 项目结构

```
AutoModeCodeGenerator/
├── README.md
├── LICENSE
└── src/
    ├── AutoModeCodeGenerator.slnx
    ├── AutoModeCodeGenerator.Analyzers/     # 源生成器（NuGet 包）
    │   ├── AutoCodeGenerator.cs             # 增量生成器入口
    │   ├── GenerationModelExtractor.cs        # 从语义模型提取生成信息
    │   ├── SourceGeneratorHelper.cs         # 生成文件头与调度
    │   ├── SourceGeneratorInfo.cs           # 内部数据模型
    │   ├── GeneratorDiagnostics.cs          # 诊断 Mccj001 / Mccj002 / Mccj003
    │   ├── EmbeddedTemplates.cs             # 嵌入特性模板
    │   ├── CodeGeneration/                  # 按 ClassStyle 分发代码输出
    │   │   ├── CodeEmitter.cs
    │   │   ├── PocoClassEmitter.cs
    │   │   ├── RecordClassEmitter.cs
    │   │   └── InpcClassEmitter.cs
    │   ├── Templates/AutoCodeAttributes.cs  # 生成器输出的特性定义（嵌入资源）
    │   ├── EntityFiles/                     # .entity.json 解析
    │   │   ├── EntityFileParser.cs
    │   │   ├── EntityFileDefinition.cs
    │   │   ├── EntityFileLocations.cs
    │   │   └── entity.schema.json           # 打包为 NuGet content/entity.schema.json
    │   ├── AutoFilesGenerator.cs            # 从 .entity.json 生成 POCO/Record/Inpc
    │   ├── README.md                        # NuGet 包使用文档（消费方）
    │   ├── PACKAGING.md                     # NuGet 发包指南（维护者）
    │   ├── NugetPackage.props               # 包元数据与打包项
    │   ├── CodeAnalyzers.props              # 分析器 DLL 输出路径
    │   ├── AnalyzerReleases.Shipped.md      # 已发布诊断规则追踪
    │   └── AnalyzerReleases.Unshipped.md    # 待发布诊断规则追踪
    ├── AutoModeCodeGenerator.Samples/       # 示例与本地验证
    │   ├── TestModel.interface.cs
    │   ├── TestModel.entity.json
    │   └── Program.cs
    └── AutoModeCodeGenerator.Tests/         # xUnit 源生成器单元测试
        ├── SourceGeneratorTestHelper.cs
        ├── PocoGenerationTests.cs
        ├── RecordGenerationTests.cs
        ├── InpcGenerationTests.cs
        ├── InterfaceInheritanceTests.cs
        ├── SyntaxValidationTests.cs
        ├── EntityFileGenerationTests.cs
        ├── AdvancedGenerationTests.cs
        ├── AutoFilesGeneratorIntegrationTests.cs
        └── DiagnosticTests.cs
```

| 项目 | 目标框架 | 说明 |
|------|----------|------|
| `AutoModeCodeGenerator.Analyzers` | netstandard2.0 | 源生成器，版本 `1.0.0.18`，构建时自动打包 |
| `AutoModeCodeGenerator.Samples` | net8.0 | 本地引用 Analyzers 的演示项目 |
| `AutoModeCodeGenerator.Tests` | net8.0 | 使用 Roslyn `GeneratorDriver` 的单元测试 |

## 安装

### NuGet

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.0.18" PrivateAssets="all" />
```

**使用文档**（安装、特性参数、`.entity.json` 字段、诊断等）见 [`src/AutoModeCodeGenerator.Analyzers/README.md`](src/AutoModeCodeGenerator.Analyzers/README.md)，该文件会作为 NuGet 包 README 发布。

### 项目引用（本地开发）

```xml
<ProjectReference Include="..\AutoModeCodeGenerator.Analyzers\AutoModeCodeGenerator.Analyzers.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

可选：将生成代码输出到磁盘，便于查看：

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
```

## 使用说明

完整的使用文档见 **[Analyzers/README.md](src/AutoModeCodeGenerator.Analyzers/README.md)**，内容包括：

- 特性方式快速开始与 Id 关联机制
- `AutoCodeClassModes`、`AutoCodeProperty`、`AutoCodeNullable`、`CustomAttributeAttribute` 参数表
- `ClassStyle`（Poco / Record / Inpc）与生成文件命名
- 诊断 `Mccj001`–`Mccj003`
- `.entity.json` 配置与 `entity.schema.json` 引用

本地可运行示例项目验证：

```bash
dotnet run --project src/AutoModeCodeGenerator.Samples/Samples.csproj
```

示例源码：[`TestModel.interface.cs`](src/AutoModeCodeGenerator.Samples/TestModel.interface.cs)、[`TestModel.entity.json`](src/AutoModeCodeGenerator.Samples/TestModel.entity.json)。

## 开发与构建

```bash
# 构建整个解决方案
dotnet build src/AutoModeCodeGenerator.slnx

# 运行单元测试
dotnet test src/AutoModeCodeGenerator.Tests/AutoModeCodeGenerator.Tests.csproj

# 运行示例
dotnet run --project src/AutoModeCodeGenerator.Samples/Samples.csproj
```

- Analyzers 项目启用 `GeneratePackageOnBuild`，输出 NuGet 包至 `bin/`。
- 测试项目通过 `CSharpGeneratorDriver` 在内存中编译测试源码并断言生成结果，无需启动完整 MSBuild。
- 测试覆盖：Poco / Record / Inpc 生成、接口继承、特性高级选项（`ToNullable`、`CustomAttribute`、类声明源等）、`.entity.json` 解析与 `AutoFilesGenerator` 集成、诊断 `Mccj001`–`Mccj003`。

### 调试源生成器

在 `AutoCodeGenerator.Initialize` 方法开头临时添加 `System.Diagnostics.Debugger.Launch();`，编译消费项目时会附加调试器。调试完成后记得移除。

## 路线图 / 已知限制

- 正式测试见 `AutoModeCodeGenerator.Tests`（含 `AutoFilesGenerator` 集成测试）。
- CI：GitHub Actions（`.github/workflows/ci.yml`）在 push/PR 时运行 `dotnet build` 与 `dotnet test`。

## 许可证

[Apache License 2.0](LICENSE)
