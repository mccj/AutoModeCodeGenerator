using System.ComponentModel;

namespace AutoModeCodeGenerator.Tests;

public class InterfaceInheritanceTests
{
    [Fact]
    public void Includes_properties_from_base_interfaces()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface IChild : IBase
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                int ChildId { get; set; }
            }

            public interface IBase
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string? Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IChild.g.cs");

        Assert.Contains("public int ChildId { get; set; }", generated);
        Assert.Contains("public string? Name { get; set; }", generated);
    }

    [Fact]
    public void Copies_source_attributes_when_inherit_attribute_is_enabled()
    {
        const string source = """
            #nullable enable
            using System.ComponentModel;

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "dto",
                Namespace = "App",
                InheritAttribute = true)]
            public interface IEntity
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                [DisplayName("sssssssssssssssss")]
                long? ProjectId { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IEntity.g.cs");

        Assert.Contains("[global::System.ComponentModel.DisplayNameAttribute", generated);
        Assert.Contains("public long? ProjectId { get; set; }", generated);
    }
}
