using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Tests;

public class DiagnosticTests
{
    [Fact]
    public void Reports_mccj002_when_property_modifiers_conflict()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface IConflict
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto", IsVirtual = true, IsOverride = true)]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d => d.Id == "Mccj002" && d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Emits_attribute_definitions_on_post_initialization()
    {
        var result = SourceGeneratorTestHelper.RunGenerator("public interface IEmpty { }");
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "AutoCodeAttribute.g.cs");

        Assert.Contains("class AutoCodeClassModesAttribute", generated);
        Assert.Contains("class AutoCodePropertyAttribute", generated);
        Assert.Contains("class AutoCodeNullableAttribute", generated);
        Assert.Contains("class CustomAttributeAttribute", generated);
    }
}
