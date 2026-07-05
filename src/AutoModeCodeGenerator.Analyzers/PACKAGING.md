# AutoModeCodeGenerator.Analyzers 发包指南

本文档说明如何构建、验证并发布 `AutoModeCodeGenerator.Analyzers` NuGet 包。

## 包内容

| 路径 | 说明 |
|------|------|
| `analyzers/dotnet/cs/AutoModeCodeGenerator.Analyzers.dll` | 源生成器与分析器程序集（消费项目不引用 `lib/`） |
| `README.md` | 消费方使用文档（安装、特性参数、entity.json 等） |
| `content/entity.schema.json` | `.entity.json` 的 JSON Schema，供编辑器补全 |

包元数据（`Description`、`PackageTags`、许可证等）定义在 `NugetPackage.props`；版本号在 `AutoModeCodeGenerator.Analyzers.csproj` 的 `<Version>` 中维护。

## 前置条件

- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 或更高版本
- 已配置目标源的 API Key（见下方「发布」）

## 版本号

在 `AutoModeCodeGenerator.Analyzers.csproj` 中更新：

```xml
<Version>1.0.0.18</Version>
```

发布前请同步更新以下文件中的版本号：

- `AutoModeCodeGenerator.Analyzers.csproj` 的 `<Version>`
- `src/AutoModeCodeGenerator.Analyzers/README.md` 安装示例
- 仓库根目录 `README.md` 安装示例与项目表

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

## 发布前检查

```bash
# 运行全部单元测试
dotnet test src/AutoModeCodeGenerator.Tests/AutoModeCodeGenerator.Tests.csproj -c Release

# 查看包内文件列表
dotnet nuget verify src/AutoModeCodeGenerator.Analyzers/bin/Release/AutoModeCodeGenerator.Analyzers.*.nupkg --all
```

确认：

- [ ] `analyzers/dotnet/cs/` 下存在分析器 DLL
- [ ] 根目录包含 `README.md`
- [ ] `content/entity.schema.json` 存在
- [ ] 版本号与 README 安装示例一致

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
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.0.18" PrivateAssets="all" />
```

## 消费方安装

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="{Version}" PrivateAssets="all" />
```

`PrivateAssets="all"` 确保生成器仅参与编译，不传递到依赖项目的运行时引用。

## 相关配置

| 文件 | 作用 |
|------|------|
| `AutoModeCodeGenerator.Analyzers.csproj` | 目标框架、版本号、嵌入资源 |
| `NugetPackage.props` | NuGet 元数据、`GeneratePackageOnBuild`、README 与 schema 打包 |
| `CodeAnalyzers.props` | `IncludeBuildOutput=false`，DLL 输出到 `analyzers/dotnet/cs` |
