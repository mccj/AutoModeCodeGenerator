namespace AutoModeCodeGenerator.Tests;

public class PocoGenerationTests
{
    [Fact]
    public void Generates_poco_class_with_namespace_and_suffix()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeNullable(Type = AutoCodeGenerator.NullableEnum.Enable)]
            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "dto",
                Namespace = "MyApp.Models",
                Suffix = "Dto")]
            public interface IUser
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Name { get; set; }

                [AutoCodeGenerator.AutoCodeProperty(Id = "dto", IsNullable = true)]
                int Age { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.NormalizeGeneratedCode(
            SourceGeneratorTestHelper.GetGeneratedSource(result, "IUser.g.cs"));

        Assert.Contains("namespace MyApp.Models", generated);
        Assert.Contains("public partial class IUserDto", generated);
        Assert.Contains("public string Name { get; set; }", generated);
        Assert.Contains("public int? Age { get; set; }", generated);
        Assert.Contains("#nullable enable", generated);
    }

    [Fact]
    public void Generates_multiple_modes_from_single_interface()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "a", Namespace = "Ns.A")]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "b", Namespace = "Ns.B", Suffix = "View")]
            public interface IItem
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "a", "b" })]
                int Id { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IItem.g.cs");

        Assert.Contains("namespace Ns.A", generated);
        Assert.Contains("public partial class IItem", generated);
        Assert.Contains("namespace Ns.B", generated);
        Assert.Contains("public partial class IItemView", generated);
    }
}
