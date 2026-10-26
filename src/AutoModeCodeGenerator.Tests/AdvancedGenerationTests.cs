using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Tests;

/// <summary>
/// 特性生成路径的高级选项与边界场景测试。
/// </summary>
public class AdvancedGenerationTests
{
    [Fact]
    public void Generates_from_class_declaration()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "m", Namespace = "App.Models")]
            public class SourceModel
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "m")]
                public int Id { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "SourceModel.g.cs");

        Assert.Contains("namespace App.Models", generated);
        Assert.Contains("public partial class SourceModel", generated);
        Assert.Contains("public int Id { get; set; }", generated);
    }

    [Fact]
    public void Applies_to_nullable_on_all_properties()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App", ToNullable = true)]
            public interface IItem
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                int Id { get; set; }

                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IItem.g.cs");

        Assert.Contains("public int? Id { get; set; }", generated);
        Assert.Contains("public string? Name { get; set; }", generated);
    }

    [Fact]
    public void Applies_default_value_to_property()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface ISettings
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto", DefaultValue = "= \"Untitled\"")]
                string Title { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "ISettings.g.cs");

        Assert.Contains("public string Title { get; set; } = \"Untitled\";", generated);
    }

    [Fact]
    public void Generates_virtual_property_when_only_virtual_set()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface IBase
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto", IsVirtual = true)]
                int Revision { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IBase.g.cs");

        Assert.Contains("public virtual int Revision { get; set; }", generated);
        SourceGeneratorTestHelper.AssertGeneratedCodeCompiles(result, "IBase.g.cs");
    }

    [Fact]
    public void Applies_custom_attribute_on_class_and_property()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.CustomAttributeAttribute(
                Id = "dto",
                Attributes = new[] { "global::System.Serializable" })]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface IProduct
            {
                [AutoCodeGenerator.CustomAttributeAttribute(
                    Id = "dto",
                    Attributes = new[] { "global::System.Obsolete(\"legacy\")" })]
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Sku { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IProduct.g.cs");

        Assert.Contains("[global::System.Serializable]", generated);
        Assert.Contains("[global::System.Obsolete(\"legacy\")]", generated);
    }

    [Fact]
    public void Emits_class_summary_from_attribute()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "dto",
                Namespace = "App",
                Summary = "用户数据传输对象")]
            public interface IUserDto
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IUserDto.g.cs");

        Assert.Contains("/// <summary>", generated);
        Assert.Contains("用户数据传输对象", generated);
    }

    [Fact]
    public void Emits_nullable_disable_directive()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeNullable(Type = AutoCodeGenerator.NullableEnum.Disable)]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface ILegacy
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Code { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "ILegacy.g.cs");

        Assert.Contains("#nullable disable", generated);
    }

    [Fact]
    public void Record_single_char_property_uses_at_prefix()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "rec",
                Namespace = "App",
                ClassStyle = AutoCodeGenerator.ClassStyleEnum.Record)]
            public interface IAxis
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "rec")]
                double X { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IAxis.g.cs");

        Assert.Contains("record class IAxis", generated);
        Assert.Contains("public IAxis(double @x)", generated);
        Assert.Contains("public double X { get; init; }", generated);
    }

    [Fact]
    public void Skips_conflicting_property_when_mccj002_reported()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface IConflict
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto", IsVirtual = true, IsOverride = true)]
                string Bad { get; set; }

                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Good { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IConflict.g.cs");
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d => d.Id == "Mccj002");
        Assert.DoesNotContain("Bad", generated);
        Assert.Contains("public string Good { get; set; }", generated);
    }

    [Fact]
    public void Emits_usings_from_class_mode_attribute()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "dto",
                Namespace = "App.Models",
                Usings = new[] { "System.Collections.Generic" })]
            public interface IListHolder
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                int Count { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IListHolder.g.cs");

        Assert.Contains("using System.Collections.Generic;", generated);
    }
}
