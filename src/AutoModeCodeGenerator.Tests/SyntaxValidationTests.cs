namespace AutoModeCodeGenerator.Tests;

public class SyntaxValidationTests
{
    [Fact]
    public void Generated_poco_output_compiles_without_errors()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App.Models")]
            public interface IProduct
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        SourceGeneratorTestHelper.AssertGeneratedCodeCompiles(result, "IProduct.g.cs");
    }

    [Fact]
    public void Generated_record_output_compiles_without_errors()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "rec",
                Namespace = "App",
                ClassStyle = AutoCodeGenerator.ClassStyleEnum.Record)]
            public interface IPoint
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "rec")]
                double XCoord { get; set; }

                [AutoCodeGenerator.AutoCodeProperty(Id = "rec")]
                double YCoord { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        SourceGeneratorTestHelper.AssertGeneratedCodeCompiles(result, "IPoint.g.cs");
    }

    [Fact]
    public void Generated_inpc_output_compiles_without_errors()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "vm",
                Namespace = "App.ViewModels",
                ClassStyle = AutoCodeGenerator.ClassStyleEnum.Inpc,
                InheritType = typeof(AutoCodeGenerator.InpcPropertyBindableBaseAbstract))]
            public interface ICounter
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "vm")]
                int Count { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        SourceGeneratorTestHelper.AssertGeneratedCodeCompiles(result, "ICounter.g.cs");
    }
}
