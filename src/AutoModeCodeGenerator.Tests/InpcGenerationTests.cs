namespace AutoModeCodeGenerator.Tests;

public class InpcGenerationTests
{
    [Fact]
    public void Generates_inpc_class_inheriting_bindable_base()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "vm",
                Namespace = "MyApp.ViewModels",
                ClassStyle = AutoCodeGenerator.ClassStyleEnum.Inpc,
                InheritType = typeof(AutoCodeGenerator.InpcPropertyBindableBaseAbstract))]
            public interface ICounter
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "vm")]
                int Count { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "ICounter.g.cs");

        Assert.Contains(
            "public partial class ICounter : global::AutoCodeGenerator.InpcPropertyBindableBaseAbstract",
            generated);
        Assert.Contains("RaisePropertyChanged()", generated);
        Assert.Contains("SetValue(value)", generated);
    }
}
