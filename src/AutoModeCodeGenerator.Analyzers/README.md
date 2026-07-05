# AutoModeCodeGenerator.Analyzers

基于 Roslyn 的 C# **增量源生成器**，在编译期根据特性或 `.entity.json` 自动生成 **Poco**、**Record**、**Inpc** 等样式的实现类。

- NuGet：<https://www.nuget.org/packages/AutoModeCodeGenerator.Analyzers>
- 源码：<https://github.com/mccj/AutoModeCodeGenerator>
- 许可证：[Apache License 2.0](https://github.com/mccj/AutoModeCodeGenerator/blob/main/LICENSE)

## 安装

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.0.18" PrivateAssets="all" />
```

`PrivateAssets="all"` 确保生成器仅参与编译，不传递到依赖项目的运行时引用。

### 可选：将生成代码输出到磁盘

便于在 IDE 中查看生成结果：

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
```

## 快速开始（特性方式）

1. 在**接口或类**上添加 `[AutoCodeClassModes]`，指定唯一 `Id` 及生成选项。
2. 在需要导出的**属性**上添加 `[AutoCodeProperty]`，通过 `Id` / `Ids` 关联到对应的类模式。
3. 编译后，生成器输出 `{命名空间}.{类型名}.g.cs`。

```csharp
#nullable enable

[AutoCodeGenerator.AutoCodeNullable(Type = AutoCodeGenerator.NullableEnum.Enable)]
[AutoCodeGenerator.AutoCodeClassModes(
    Id = "dto",
    Namespace = "MyApp.Models",
    Suffix = "Dto",
    ClassStyle = AutoCodeGenerator.ClassStyleEnum.Poco)]
public interface IUser
{
    [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
    string Name { get; set; }

    [AutoCodeGenerator.AutoCodeProperty(Id = "dto", IsNullable = true)]
    int Age { get; set; }
}
```

编译后生成类似：

```csharp
namespace MyApp.Models
{
    public partial class IUserDto
    {
        public string Name { get; set; } = null!;
        public int? Age { get; set; }
    }
}
```

完整示例见仓库中的 [TestModel.interface.cs](https://github.com/mccj/AutoModeCodeGenerator/blob/main/src/AutoModeCodeGenerator.Samples/TestModel.interface.cs)。

## 核心概念

### Id 关联机制

- `AutoCodeClassModes(Id = "a1", ...)` 定义一种生成模式。
- `AutoCodeProperty(Id = "a1")` 或 `Ids = new[] { "a1", "a2" }` 将属性绑定到一种或多种模式。
- 同一接口可配置多个 `AutoCodeClassModes`，一次定义生成多个不同命名空间/样式的类。

### 类样式（ClassStyle）

| 样式 | 说明 |
|------|------|
| `Poco` | 普通 `{ get; set; }` 属性（默认） |
| `Record` | 带主构造函数的 Record 风格类，属性为 `{ get; }` |
| `Inpc` | 基于 `InpcPropertyBindableBaseAbstract` 的属性，setter 内调用 `RaisePropertyChanged()` |

**Inpc** 需指定基类：

```csharp
[AutoCodeClassModes(
    Id = "vm",
    ClassStyle = AutoCodeGenerator.ClassStyleEnum.Inpc,
    InheritType = typeof(AutoCodeGenerator.InpcPropertyBindableBaseAbstract))]
```

**Record** 会为每个属性生成构造函数参数，参数名为属性名首字母小写（单字符属性保持原样，如 `X` → `@X`）。

### 继承与接口成员

- 源类型继承的其他接口成员上的 `AutoCodeProperty` 也会参与生成。
- `InheritAttribute = true` 时，源属性上的非生成器 Attribute（如 `[DisplayName]`）会复制到生成类。

### 生成文件命名

| 来源 | 输出文件 |
|------|----------|
| 特性标注 | `{Namespace}.{SourceTypeName}.g.cs`（按模式生成的类在同一文件内） |
| `.entity.json` | `{ClassName}.entity.g.cs` |

## 特性参考

安装包后，生成器会自动注入 `AutoCodeGenerator` 命名空间下的特性定义，无需手动引用程序集。

### AutoCodeClassModesAttribute

标注在**接口或类**上，可多次使用。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` | `string` | **必填**，与属性的 `Id` / `Ids` 关联 |
| `Name` | `string` | 类名，默认取源类型名 |
| `Prefix` / `Suffix` | `string` | 类名前后缀 |
| `Namespace` | `string` | 目标命名空间 |
| `NamespacePrefix` / `NamespaceSuffix` | `string` | 命名空间前后缀 |
| `Modifier` | `AccessibilityEnum` | 访问修饰符，默认 `Public` |
| `ClassStyle` | `ClassStyleEnum` | `Poco` / `Record` / `Inpc` |
| `InheritStr` / `InheritType` | `string` / `Type` | 基类 |
| `InterfaceStrs` / `InterfaceTypes` | `string[]` / `Type[]` | 实现的接口 |
| `Usings` | `string[]` | 生成文件顶部的 `using` |
| `IsAbstract` | `bool` | 是否 `abstract` |
| `IsPartial` | `bool` | 是否 `partial`，默认 `true` |
| `InheritAttribute` | `bool` | 是否继承源属性上的外部 Attribute |
| `ToNullable` | `bool` | 是否将所有属性类型标记为可空 |
| `Attributes` | `string[]` | 附加到类上的 Attribute 字符串 |
| `Summary` / `Example` / `Remarks` | `string` | XML 文档 |

### AutoCodePropertyAttribute

标注在**属性**上。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` / `Ids` | `string` / `string[]` | 关联的类模式 Id |
| `Name` | `string` | 属性名，默认取源属性名 |
| `Prefix` / `Suffix` | `string` | 属性名前后缀 |
| `PropertyTypeStr` / `PropertyType` | `string` / `Type` | 覆盖属性类型 |
| `Modifier` | `AccessibilityEnum` | 访问修饰符 |
| `IsVirtual` / `IsOverride` / `IsNew` | `bool` | 虚 / 重写 / 隐藏，**三者互斥** |
| `IsNullable` | `bool` | 是否可空 |
| `DefaultValue` | `string` | 默认值表达式，如 `"= \"\""` |
| `Attributes` | `string[]` | 附加 Attribute |
| `Summary` / `Example` / `Remarks` | `string` | XML 文档 |

### AutoCodeNullableAttribute

标注在**接口或类**上，控制生成文件的 `#nullable` 指令（不可多次使用）。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Type` | `NullableEnum` | 见下表 |

**NullableEnum 取值：**

| 值 | 效果 |
|----|------|
| `Disable` | `#nullable disable` |
| `Enable` | `#nullable enable` |
| `Restore` | `#nullable restore` |
| `DisableAnnotations` | `#nullable disable annotations` |
| `EnableAnnotations` | `#nullable enable annotations` |
| `RestoreAnnotations` | `#nullable restore annotations` |
| `DisableWarnings` | `#nullable disable warnings` |
| `EnableWarnings` | `#nullable enable warnings` |
| `RestoreWarnings` | `#nullable restore warnings` |

### CustomAttributeAttribute

按 Id 为类或属性批量附加 Attribute 字符串，可与 `AutoCodeClassModes` / `AutoCodeProperty` 配合使用。

| 属性 | 类型 | 说明 |
|------|------|------|
| `Id` / `Ids` | `string` / `string[]` | 关联的类模式 Id |
| `Attributes` | `string[]` | 要附加的 Attribute 字符串 |

### AccessibilityEnum

| 值 | 对应修饰符 |
|----|------------|
| `NotApplicable` | 无 |
| `Private` | `private` |
| `ProtectedAndInternal` | `private protected` |
| `Protected` | `protected` |
| `Internal` | `internal` |
| `ProtectedOrInternal` | `protected internal` |
| `Public` | `public` |

## 诊断

| ID | 严重性 | 说明 |
|----|--------|------|
| `Mccj001` | Error | 为指定类型生成代码时发生未处理异常 |
| `Mccj002` | Error | 同一属性的 `IsVirtual`、`IsOverride`、`IsNew` 不能同时为 true |
| `Mccj003` | Error | 处理 `.entity.json` 时解析或生成失败 |

## 从 entity.json 生成

将 JSON 文件标记为 `AdditionalFiles`，文件名以 `.entity.json` 结尾：

```xml
<AdditionalFiles Include="Models\Customer.entity.json" />
```

### 示例

```json
{
  "$schema": "https://raw.githubusercontent.com/mccj/AutoModeCodeGenerator/main/src/AutoModeCodeGenerator.Analyzers/EntityFiles/entity.schema.json",
  "namespace": "MyApp.Entities",
  "className": "Customer",
  "classStyle": "Poco",
  "nullable": "enable",
  "summary": "Customer entity",
  "properties": [
    { "name": "Id", "type": "int", "summary": "主键" },
    { "name": "Name", "type": "string", "isNullable": true }
  ]
}
```

NuGet 包内附带 `content/entity.schema.json`。可将该文件复制到项目中，并在 JSON 里通过相对路径引用 `$schema`，以获得编辑器补全与校验。

完整示例见 [TestModel.entity.json](https://github.com/mccj/AutoModeCodeGenerator/blob/main/src/AutoModeCodeGenerator.Samples/TestModel.entity.json)。

### JSON 字段参考

| 字段 | 说明 |
|------|------|
| `namespace` | 目标命名空间，默认 `Generated.Entities` |
| `className` | 类名，默认取文件名（`Order.entity.json` → `Order`） |
| `nullable` | `#nullable` 指令，默认 `enable` |
| `classStyle` | `Poco`（默认）、`Record`、`Inpc` |
| `modifier` | 类访问级别：`public`（默认）、`internal`、`protected`、`private`、`protected internal`、`private protected` |
| `isAbstract` | 是否抽象类，默认 `false` |
| `isPartial` | 是否 `partial`，默认 `true` |
| `inherit` | 基类；Inpc 默认 `InpcPropertyBindableBaseAbstract` |
| `toNullable` | 为 `true` 时所有属性类型标记为可空 |
| `inheritAttribute` | 是否继承属性上的外部特性 |
| `usings` | 生成文件顶部的 `using` 列表 |
| `interfaces` | 实现的接口列表 |
| `attributes` | 类级别自定义特性（字符串，如 `global::System.Serializable`） |
| `prefix` / `suffix` | 类名前后缀 |
| `summary` / `remarks` / `example` | 类级 XML 文档 |
| `properties[].name` / `type` | 属性名与类型（**必填**） |
| `properties[].isNullable` | 是否可空 |
| `properties[].modifier` | 属性访问级别，默认 `public` |
| `properties[].defaultValue` | 初始值表达式（如 `= \"Untitled\"`） |
| `properties[].isVirtual` / `isOverride` / `isNew` | 属性修饰符 |
| `properties[].attributes` | 属性级别自定义特性 |
| `properties[].inheritAttributes` | 属性继承特性（需配合 `inheritAttribute: true`） |
| `properties[].prefix` / `suffix` | 属性名前后缀 |
| `properties[].summary` / `remarks` / `example` | 属性级 XML 文档 |

字段完整定义见包内 `content/entity.schema.json` 或仓库 [entity.schema.json](https://github.com/mccj/AutoModeCodeGenerator/blob/main/src/AutoModeCodeGenerator.Analyzers/EntityFiles/entity.schema.json)。

## 验证与测试

仓库提供 `AutoModeCodeGenerator.Tests` 项目，通过 Roslyn `GeneratorDriver` 在内存中验证生成结果与诊断行为：

```bash
dotnet test src/AutoModeCodeGenerator.Tests/AutoModeCodeGenerator.Tests.csproj
```

主要测试类：

| 测试类 | 覆盖内容 |
|--------|----------|
| `PocoGenerationTests` | 基础 POCO、多模式生成 |
| `RecordGenerationTests` / `InpcGenerationTests` | Record 与 Inpc 样式 |
| `AdvancedGenerationTests` | 类声明源、`ToNullable`、`CustomAttribute`、默认值、可空指令等 |
| `InterfaceInheritanceTests` | 接口继承与特性复制 |
| `EntityFileGenerationTests` | `.entity.json` 字段与编译验证 |
| `AutoFilesGeneratorIntegrationTests` | 增量生成器与 `Mccj003` |
| `DiagnosticTests` | `Mccj002` 与特性模板注入 |
