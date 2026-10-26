# AutoModeCodeGenerator.Conventions.SqlSugar

可选约定包：**不依赖** SqlSugar 程序集，只提供特性字符串模板与 mixin 说明。

主生成器包 `AutoModeCodeGenerator.Analyzers`（**≥ 1.0.1.0**）保持 ORM 无关。完整可复制模板：[docs/CONVENTIONS_TEMPLATE.md](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/CONVENTIONS_TEMPLATE.md)。

## 安装

```xml
<PackageReference Include="AutoModeCodeGenerator.Analyzers" Version="1.0.1.0" PrivateAssets="all" />
<PackageReference Include="AutoModeCodeGenerator.Conventions.SqlSugar" Version="1.0.1.0" />
```

## 用法

```csharp
using AutoCodeGenerator;
using AutoModeCodeGenerator.Conventions.SqlSugar;

[AutoCodeCrudModes(EntityAttributes = [SqlSugarAttrTemplates.Table("product")])]
[AutoCodeIndex("IX_Product_Name", Spec = "Name:Asc")]
public interface Product /* : IHasLongId */
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    [AutoCodeColumn(Length = 128)]
    // 或: Attributes = [SqlSugarAttrTemplates.StringLength128]
    string? Name { get; set; }

    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDto)]
    [AutoCodeColumn(ExtraAttributes = [
        SqlSugarAttrTemplates.DateTimeConverter("global::MyApp.DateTimeConverter")])]
    DateTime? CreatedTime { get; set; }
}
```

### API 一览

| API | 用途 |
|-----|------|
| `Table` / `Column` / `Navigate*` | 表、列名、导航 |
| `StringLength64/128/1024/Max`、`BigString` | 列宽（优先也可用主包 `AutoCodeColumn`） |
| `Index(...)` / `DateTimeConverter(...)` | SugarIndex、日期 DbType 转换器 |
| `PrimaryKey` / `EnumColumn` / `OpenApiStringEnum` | 主键与枚举 |
| `MixinSnippets.*` | CreateTime / Entity-only 等**可复制**注释示例 |

### CreateTime 注意

`ICreateTimeEntity` **alone 不够**——标记接口不会投影属性（**Mccj011**）。任选其一：

1. 复制 `IHasCreateTime` mixin（见 Conventions 模板），或  
2. `[AutoCodeCrudModes(InjectMarkerMembers = true, SearchRangeTemplate = "SearchRangeForStructValue<{0}>")]`

路线图与非目标：[docs/ROADMAP.md](https://github.com/mccj/AutoModeCodeGenerator/blob/main/docs/ROADMAP.md)。
