using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Tests;

public class InjectAndDedupTests
{
    [Fact]
    public void Dedupes_detail_property_when_inherited_from_dto()
    {
        const string source = """
            #nullable enable
            namespace App;

            [AutoCodeGenerator.AutoCodeCrudModes]
            public interface Item
            {
                [AutoCodeGenerator.AutoCodeProperty(ModeFlags = AutoCodeGenerator.AutoCodeModeFlags.EntityDtoDetail)]
                string Content { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "Item");

        Assert.Contains("class ItemDto", generated);
        Assert.Contains("class ItemDetailOutput", generated);
        Assert.Contains(": ItemDto", generated);

        // Detail should NOT redeclare Content (inherited from Dto)
        var detailStart = generated.IndexOf("class ItemDetailOutput", StringComparison.Ordinal);
        var detailEnd = generated.IndexOf("class ItemSearchInput", StringComparison.Ordinal);
        Assert.True(detailStart >= 0 && detailEnd > detailStart);
        var detailBody = generated.Substring(detailStart, detailEnd - detailStart);
        Assert.DoesNotContain("Content", detailBody);
        Assert.Contains("Content", generated); // still on Dto/Entity
    }

    [Fact]
    public void InjectMarkerMembers_projects_create_time()
    {
        const string source = """
            #nullable enable
            namespace App;

            public interface ICreateTimeEntity<T>
            {
                System.DateTime? CreateTime { get; set; }
            }

            [AutoCodeGenerator.AutoCodeCrudModes(
                EntityInterfaceTypes = new[] { typeof(ICreateTimeEntity<long>) },
                InjectMarkerMembers = true,
                SearchRangeTemplate = "SearchRangeForStructValue<{0}>")]
            public interface Log
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto" })]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);
        Assert.DoesNotContain(diagnostics, d => d.Id == "Mccj011");

        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "Log");
        Assert.Contains("CreateTime", generated);
        Assert.Contains("SearchRangeForStructValue<", generated);
    }

    [Fact]
    public void Reports_mccj014_when_relation_target_missing()
    {
        const string source = """
            #nullable enable
            namespace App;

            [AutoCodeGenerator.AutoCodeCrudModes]
            public interface Order
            {
                [AutoCodeGenerator.AutoCodeRelation]
                long Category { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d =>
            d.Id == "Mccj014"
            && d.GetMessage().Contains("Category"));
    }

    [Fact]
    public void Analyzer_reports_mccj012_for_six_class_modes()
    {
        const string source = """
            #nullable enable
            namespace App;

            [AutoCodeGenerator.AutoCodeClassModes(Id = "Dto", Suffix = "Dto")]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "Entity", Suffix = "Record")]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "CreateInput", Suffix = "CreateInput", InheritDto = true)]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "UpdateInput", Suffix = "UpdateInput", InheritDto = true)]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "DetailOutput", Suffix = "DetailOutput", InheritDto = true)]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "SearchInput", Suffix = "SearchInput", ToNullable = true)]
            public interface Product
            {
                [AutoCodeGenerator.AutoCodeProperty(ModeFlags = AutoCodeGenerator.AutoCodeModeFlags.EntityDtoSearch)]
                string Title { get; set; }
            }
            """;

        var diagnostics = SourceGeneratorTestHelper.RunAnalyzer(source);
        Assert.Contains(diagnostics, d => d.Id == "Mccj012");
    }

    [Fact]
    public void CodeFix_replaces_six_modes_with_crud_modes()
    {
        const string source = """
            #nullable enable
            namespace App;

            [AutoCodeGenerator.AutoCodeClassModes(Id = "Dto", Suffix = "Dto")]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "Entity", Suffix = "Record", InterfaceTypes = new[] { typeof(System.IDisposable) })]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "CreateInput", Suffix = "CreateInput", InheritDto = true)]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "UpdateInput", Suffix = "UpdateInput", InheritDto = true)]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "DetailOutput", Suffix = "DetailOutput", InheritDto = true)]
            [AutoCodeGenerator.AutoCodeClassModes(Id = "SearchInput", Suffix = "SearchInput", ToNullable = true)]
            public interface Product
            {
                [AutoCodeGenerator.AutoCodeProperty(ModeFlags = AutoCodeGenerator.AutoCodeModeFlags.EntityDtoSearch)]
                string Title { get; set; }
            }
            """;

        var fixedSource = SourceGeneratorTestHelper.ApplyCodeFix(source, "Mccj012");
        Assert.Contains("AutoCodeCrudModes", fixedSource);
        Assert.Contains("EntityInterfaceTypes", fixedSource);
        Assert.DoesNotContain("AutoCodeClassModes", fixedSource);
    }

    [Fact]
    public void Msbuild_search_range_template_applies_to_morph()
    {
        const string source = """
            #nullable enable
            namespace App;

            [AutoCodeGenerator.AutoCodeCrudModes]
            public interface Timed
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto" })]
                [AutoCodeGenerator.AutoCodeMorph(Kind = AutoCodeGenerator.AutoCodeMorphKind.SearchRange)]
                System.DateTime Created { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(
            source,
            analyzerConfigOptions: new Dictionary<string, string>
            {
                ["build_property.AutoModeSearchRangeTemplate"] = "SearchRangeForStructValue<{0}>"
            });

        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "Timed");
        Assert.Contains("SearchRangeForStructValue<", generated);
    }

    [Fact]
    public void SearchRangeType_unbound_generic_applies_to_morph()
    {
        const string source = """
            #nullable enable
            namespace App;

            public struct SearchRangeForStructValue<T>
            {
                public T? StartValue { get; set; }
                public T? EndValue { get; set; }
            }

            [AutoCodeGenerator.AutoCodeCrudModes(
                SearchRangeType = typeof(SearchRangeForStructValue<>))]
            public interface Timed
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto" })]
                [AutoCodeGenerator.AutoCodeMorph(Kind = AutoCodeGenerator.AutoCodeMorphKind.SearchRange)]
                System.DateTime Created { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "Timed");
        Assert.Contains("SearchRangeForStructValue<", generated);
        Assert.Contains("DateTime", generated);
    }

    [Fact]
    public void SearchRangeTemplate_string_wins_over_SearchRangeType()
    {
        const string source = """
            #nullable enable
            namespace App;

            public struct SearchRangeForStructValue<T> { }
            public struct OtherRange<T> { }

            [AutoCodeGenerator.AutoCodeCrudModes(
                SearchRangeType = typeof(SearchRangeForStructValue<>),
                SearchRangeTemplate = "OtherRange<{0}>")]
            public interface Timed
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto" })]
                [AutoCodeGenerator.AutoCodeMorph(Kind = AutoCodeGenerator.AutoCodeMorphKind.SearchRange)]
                int Amount { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "Timed");
        Assert.Contains("OtherRange<", generated);
        Assert.DoesNotContain("SearchRangeForStructValue<", generated);
    }

    [Fact]
    public void Morph_RangeType_overrides_class_default()
    {
        const string source = """
            #nullable enable
            namespace App;

            public struct SearchRangeForStructValue<T> { }
            public struct LocalRange<T> { }

            [AutoCodeGenerator.AutoCodeCrudModes(
                SearchRangeType = typeof(SearchRangeForStructValue<>))]
            public interface Timed
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto" })]
                [AutoCodeGenerator.AutoCodeMorph(
                    Kind = AutoCodeGenerator.AutoCodeMorphKind.SearchRange,
                    RangeType = typeof(LocalRange<>))]
                decimal Amount { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "Timed");
        Assert.Contains("LocalRange<", generated);
    }
}
