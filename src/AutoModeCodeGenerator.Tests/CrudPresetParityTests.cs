namespace AutoModeCodeGenerator.Tests;

public class CrudPresetParityTests
{
    [Fact]
    public void Crud_modes_preset_matches_handwritten_six_mode_shape()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            public interface IEntityMarker { }

            public interface IHasId
            {
                [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Update, AutoCodeCrud.Detail })]
                long Id { get; set; }
            }

            [AutoCodeCrudModes(EntityInterfaceStrs = new[] { "IEntityMarker" })]
            public interface Product : IHasId
            {
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search })]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Product.g.cs");

        Assert.Contains("class ProductRecord", generated);
        Assert.Contains("class ProductDto", generated);
        Assert.Contains("class ProductCreateInput : ProductDto", generated);
        Assert.Contains("class ProductUpdateInput : ProductDto", generated);
        Assert.Contains("class ProductDetailOutput : ProductDto", generated);
        Assert.Contains("class ProductSearchInput", generated);
        Assert.Contains("IEntityMarker", generated);
        Assert.Contains("string? Name", generated);
    }

    [Fact]
    public void Handwritten_class_mode_overrides_same_id_from_preset()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeCrudModes]
            [AutoCodeClassModes(Id = AutoCodeCrud.Entity, Suffix = "Entity")]
            public interface Item
            {
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Dto })]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Item.g.cs");

        Assert.Contains("class ItemEntity", generated);
        Assert.DoesNotContain("class ItemRecord", generated);
        Assert.Contains("class ItemDto", generated);
    }

    [Fact]
    public void Omit_modes_entity_only()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeCrudModes(OmitModes = AutoCodeCrudOmit.EntityOnly)]
            public interface Map
            {
                [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
                long A { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Map.g.cs");

        Assert.Contains("class MapRecord", generated);
        Assert.DoesNotContain("MapDto", generated);
        Assert.DoesNotContain("CreateInput", generated);
    }

    [Fact]
    public void AutoCodeRelation_expands_fk_shapes()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeCrudModes]
            public interface Category
            {
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search })]
                string Name { get; set; }
            }

            [AutoCodeCrudModes]
            public interface Product
            {
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search })]
                string Name { get; set; }

                [AutoCodeRelation(TargetName = nameof(Category), IncludeNavigate = false)]
                long Category { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Product.g.cs");

        Assert.Contains("CategoryId", generated);
        Assert.Contains("CategoryDetailOutput", generated);
        Assert.Contains("CategorySearchInput", generated);
        Assert.DoesNotContain("SqlSugar.Navigate", generated);
        SourceGeneratorTestHelper.AssertGeneratedCodeCompiles(result, "Product.g.cs", "Category.g.cs");
    }

    [Fact]
    public void AutoCodeRelation_include_navigate_emits_attribute_string()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeCrudModes]
            public interface Product
            {
                [AutoCodeRelation(TargetName = "Category", IncludeNavigate = true)]
                long Category { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Product.g.cs");
        Assert.Contains("SqlSugar.Navigate", generated);
        Assert.Contains("CategoryRecord", generated);
    }
}
