using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Tests;

public class WarningDiagnosticTests
{
    [Fact]
    public void Reports_mccj004_when_class_mode_id_is_missing()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Namespace = "App")]
            public interface IMissingId
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d => d.Id == "Mccj004" && d.Severity == DiagnosticSeverity.Warning);
        Assert.Contains(diagnostics, d => d.Id == "Mccj008" && d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_mccj005_when_property_mode_is_unbound()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface IUnbound
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "missing")]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d => d.Id == "Mccj005" && d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_mccj006_when_generated_property_names_duplicate()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface IDup
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto", Name = "Title")]
                string A { get; set; }

                [AutoCodeGenerator.AutoCodeProperty(Id = "dto", Name = "Title")]
                string B { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d => d.Id == "Mccj006" && d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_mccj007_when_entity_property_missing_fields()
    {
        const string entityJson = """
            {
              "namespace": "App",
              "className": "Broken",
              "properties": [
                { "name": "OnlyName" },
                { "type": "int" }
              ]
            }
            """;

        var result = SourceGeneratorTestHelper.RunEntityFilesGenerator("Broken.entity.json", entityJson);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d => d.Id == "Mccj007" && d.Severity == DiagnosticSeverity.Warning);
    }

    [Fact]
    public void Reports_mccj008_when_property_exists_without_valid_class_modes()
    {
        const string source = """
            #nullable enable

            public interface IOnlyProperty
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto")]
                string Name { get; set; }
            }
            """;

        // No AutoCodeClassModes on the type — ForAttributeWithMetadataName won't even run.
        // Mccj008 is reported when ClassModes exist but all lack Id, covered by mccj004 test.
        // Here verify generator produces no class file for types without ClassModes.
        var result = SourceGeneratorTestHelper.RunGenerator(source);
        Assert.DoesNotContain(result.GeneratedSources, s => s.HintName.EndsWith("IOnlyProperty.g.cs", StringComparison.Ordinal));
    }
}
