# AutoModeCodeGenerator.Analyzers 发包指南

本文档说明如何构建、验证并发布 **Analyzers** 与可选 **Conventions.SqlSugar** NuGet 包。

消费方使用说明见同目录 [README.md](README.md)；版本历史见仓库根目录 [CHANGELOG.md](../../CHANGELOG.md)；管线见 [ARCHITECTURE.md](../../docs/ARCHITECTURE.md)。

## 包内容

| 路径 | 说明 |
|------|------|
| `analyzers/dotnet/cs/AutoModeCodeGenerator.Analyzers.dll` | 源生成器与分析器程序集（消费项目不引用 `lib/`） |
| `analyzers/dotnet/cs/System.Text.Json.dll` | entity.json 解析 |
| `analyzers/dotnet/cs/System.Text.Encodings.Web.dll` | STJ 依赖 |
| `analyzers/dotnet/cs/System.IO.Pipelines.dll` | STJ 依赖 |
| `analyzers/dotnet/cs/System.Memory.dll` | STJ 依赖 |
| `analyzers/dotnet/cs/System.Buffers.dll` | STJ 依赖 |
| `analyzers/dotnet/cs/System.Runtime.CompilerServices.Unsafe.dll` | STJ 依赖 |
| `analyzers/dotnet/cs/System.Threading.Tasks.Extensions.dll` | STJ 依赖 |
| `analyzers/dotnet/cs/System.Numerics.Vectors.dll` | STJ 依赖 |
| `build/AutoModeCodeGenerator.Analyzers.props` | MSBuild 开关（`AutoModeGenerateEntityFiles` 等） |
| `README.md` | 消费方使用文档（安装、特性参数、entity.json 等） |
| `content/entity.schema.json` | `.entity.json` 的 JSON Schema，供编辑器补全 |

包元数据（`Description`、`PackageTags`、许可证等）定义在 `NugetPackage.props`；版本号在 `AutoModeCodeGenerator.Analyzers.csproj` 的 `<Version>` 中维护。STJ 依赖路径由 `CodeAnalyzers.props` 打入 `analyzers/dotnet/cs`。

## 前置条件

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 或更高版本
- 已配置目标源的 API Key（见下方「发布」）

## 版本号

- **仓库基线**（本地 / GitHub CI 校验）：`AutoModeCodeGenerator.Analyzers.csproj` 的 `<Version>1.0.1.0</Version>`
- **AppVeyor 发包**：Build version format = `1.0.1.{build}`，Next build number 从 `0` 起 → 首包 `1.0.1.0`；`dotnet pack` 时用 `/p:Version=%APPVEYOR_BUILD_VERSION%` 覆盖 csproj

```xml
<Version>1.0.1.0</Version>
```

发布前请同步更新：

- `AutoModeCodeGenerator.Analyzers.csproj` 与 `AutoModeCodeGenerator.Conventions.SqlSugar.csproj` 的 `<Version>`（保持同号）
- `src/AutoModeCodeGenerator.Analyzers/README.md` 安装示例
- 仓库根目录 `README.md` 安装示例与项目表
- 仓库根目录 `CHANGELOG.md`（新增版本条目）

CI 会校验 Analyzers csproj 与两个 README 中的版本字符串一致。

## 本地构建与打包

```bash
# 仅构建（启用 GeneratePackageOnBuild 时会在 bin/ 下生成 .nupkg）
dotnet build src/AutoModeCodeGenerator.Analyzers/AutoModeCodeGenerator.Analyzers.csproj -c Release

# 或显式打包（不依赖 GeneratePackageOnBuild）
dotnet pack src/AutoModeCodeGenerator.Analyzers/AutoModeCodeGenerator.Analyzers.csproj -c Release --no-build
```

输出路径示例：

```
src/AutoModeCodeGenerator.Analyzers/bin/Release/AutoModeCodeGenerator.Analyzers.{Version}.nupkg
```

查看包内文件（Linux / macOS / Git Bash）：

```bash
unzip -l src/AutoModeCodeGenerator.Analyzers/bin/Release/AutoModeCodeGenerator.Analyzers.*.nupkg
```

PowerShell：

```powershell
Add-Type -AssemblyName System.IO.Compression.FileSystem
$nupkg = Get-Item src/AutoModeCodeGenerator.Analyzers/bin/Release/AutoModeCodeGenerator.Analyzers.*.nupkg
[IO.Compression.ZipFile]::OpenRead($nupkg.FullName).Entries.FullName
```

## 发布前检查

```bash
# 运行全部单元测试
dotnet test src/AutoModeCodeGenerator.Tests/AutoModeCodeGenerator.Tests.csproj -c Release

# 可选：nuget verify
dotnet nuget verify src/AutoModeCodeGenerator.Analyzers/bin/Release/AutoModeCodeGenerator.Analyzers.*.nupkg --all
```

确认：

- [ ] `analyzers/dotnet/cs/` 下存在分析器 DLL
- [ ] `analyzers/dotnet/cs/` 下存在 `System.Text.Json.dll` 及其传递依赖
- [ ] `build/AutoModeCodeGenerator.Analyzers.props` 存在
- [ ] 根目录包含 `README.md`
- [ ] `content/entity.schema.json` 存在
- [ ] 版本号与 README 安装示例一致
- [ ] `CHANGELOG.md` 已写入本版本说明（含破坏性变更）
- [ ] `AnalyzerReleases.Unshipped.md` 中的新规则已按发布流程处理（发布后迁入 Shipped）

单元测试 `PackageContentTests` 也会断言 nupkg 含分析器 DLL、STJ 与 build props。

## 发布

### NuGet.org（正式源）

```bash
dotnet nuget push src/AutoModeCodeGenerator.Analyzers/bin/Release/AutoModeCodeGenerator.Analyzers.*.nupkg \
  --api-key <NUGET_API_KEY> \
  --source https://api.nuget.org/v3/index.json
```

包页面：<https://www.nuget.org/packages/AutoModeCodeGenerator.Analyzers>

### MyGet（预发布源）

```bash
dotnet nuget push src/AutoModeCodeGenerator.Analyzers/bin/Release/AutoModeCodeGenerator.Analyzers.*.nupkg \
  --api-key <MYGET_API_KEY> \
  --source https://www.myget.org/F/mccj/api/v2/package
```

在消费项目中添加源后安装预发布版本：

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.1.0" PrivateAssets="all" />
```

也可使用仓库 [`.github/workflows/release.yml`](../../.github/workflows/release.yml) 自动化发布（若已配置密钥）。

## Conventions.SqlSugar（可选包）

与 Analyzers **同版本号**发布。无 SqlSugar 程序集依赖，仅提供字符串模板 / mixin 源码约定。

```bash
dotnet pack src/AutoModeCodeGenerator.Conventions.SqlSugar/AutoModeCodeGenerator.Conventions.SqlSugar.csproj -c Release
dotnet nuget push src/AutoModeCodeGenerator.Conventions.SqlSugar/bin/Release/AutoModeCodeGenerator.Conventions.SqlSugar.*.nupkg \
  --api-key <NUGET_API_KEY> \
  --source https://api.nuget.org/v3/index.json
```

说明见 [Conventions.SqlSugar/README.md](../AutoModeCodeGenerator.Conventions.SqlSugar/README.md)。

## 消费方安装

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="{Version}" PrivateAssets="all" />
```

`PrivateAssets="all"` 确保生成器仅参与编译，不传递到依赖项目的运行时引用。

## 相关配置

| 文件 | 作用 |
|------|------|
| `AutoModeCodeGenerator.Analyzers.csproj` | 目标框架、版本号、嵌入资源、STJ PackageReference |
| `NugetPackage.props` | NuGet 元数据、`GeneratePackageOnBuild`、README / schema / build props 打包 |
| `CodeAnalyzers.props` | `IncludeBuildOutput=false`，主 DLL 与 STJ 依赖输出到 `analyzers/dotnet/cs` |
| `build/AutoModeCodeGenerator.Analyzers.props` | 消费端 MSBuild 默认属性与 `CompilerVisibleProperty` |
| `AnalyzerReleases.Shipped.md` / `Unshipped.md` | 诊断规则发布追踪 |
