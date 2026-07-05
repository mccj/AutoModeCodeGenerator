namespace AutoModeCodeGenerator.Tests;

public class RecordGenerationTests
{
    [Fact]
    public void Generates_record_style_class_with_primary_constructor()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "rec",
                Namespace = "MyApp",
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
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IPoint.g.cs");

        Assert.Contains("public IPoint(double @xCoord", generated);
        Assert.Contains("double @yCoord)", generated);
        Assert.Contains("this.XCoord = @xCoord;", generated);
        Assert.Contains("this.YCoord = @yCoord;", generated);
        Assert.Contains("public double XCoord { get; }", generated);
        Assert.Contains("public double YCoord { get; }", generated);
    }
}
