using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Tests;

public class AutoFilesGeneratorIntegrationTests
{
    [Fact]
    public void Generates_source_from_additional_text_via_AutoFilesGenerator()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "Widget",
              "properties": [
                { "name": "Code", "type": "string" }
              ]
            }
            """;

        var result = SourceGeneratorTestHelper.RunEntityFilesGenerator("Widget.entity.json", entityJson);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Widget.entity.g.cs");

        Assert.Contains("public partial class Widget", generated);
        Assert.Contains("public string Code { get; set; }", generated);
        Assert.Empty(result.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning));
    }

    [Fact]
    public void Reports_mccj003_for_invalid_json_via_AutoFilesGenerator()
    {
        const string entityJson = "{ invalid json";

        var result = SourceGeneratorTestHelper.RunEntityFilesGenerator("Broken.entity.json", entityJson);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d => d.Id == "Mccj003" && d.Severity == DiagnosticSeverity.Error);
        Assert.NotEqual(Location.None, diagnostics.First(d => d.Id == "Mccj003").Location);
    }

    [Fact]
    public void Reports_mccj002_for_conflicting_property_modifiers_in_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "Conflict",
              "properties": [
                {
                  "name": "Title",
                  "type": "string",
                  "isVirtual": true,
                  "isOverride": true
                }
              ]
            }
            """;

        var result = SourceGeneratorTestHelper.RunEntityFilesGenerator("Conflict.entity.json", entityJson);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d => d.Id == "Mccj002" && d.Severity == DiagnosticSeverity.Error);

        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Conflict.entity.g.cs");
        Assert.DoesNotContain("Title", generated);
    }
}
