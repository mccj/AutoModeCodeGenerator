namespace AutoModeCodeGenerator.Tests;

public class CrudProductionParityTests
{
    [Fact]
    public void Six_mode_crud_header_generates_expected_types()
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

            [AutoCodeClassModes(Id = AutoCodeCrud.Dto, Suffix = AutoCodeCrud.DefaultDtoSuffix)]
            [AutoCodeClassModes(Id = AutoCodeCrud.Entity, Suffix = AutoCodeCrud.DefaultEntitySuffix, InterfaceStrs = new[] { "IEntityMarker" })]
            [AutoCodeClassModes(Id = AutoCodeCrud.Create, InheritStr = nameof(Product) + "Dto", Suffix = AutoCodeCrud.DefaultCreateSuffix, SummaryPrefix = "创建")]
            [AutoCodeClassModes(Id = AutoCodeCrud.Update, InheritStr = nameof(Product) + "Dto", Suffix = AutoCodeCrud.DefaultUpdateSuffix, SummaryPrefix = "更新")]
            [AutoCodeClassModes(Id = AutoCodeCrud.Detail, InheritStr = nameof(Product) + "Dto", Suffix = AutoCodeCrud.DefaultDetailSuffix, SummaryPrefix = "详情")]
            [AutoCodeClassModes(Id = AutoCodeCrud.Search, ToNullable = true, Suffix = AutoCodeCrud.DefaultSearchSuffix, SummaryPrefix = "搜索")]
            public interface Product : IHasId
            {
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search })]
                string Name { get; set; }

                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search }, Suffix = "Id")]
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Detail }, PropertyTypeStr = "string", IsNullable = true)]
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Search }, PropertyType = typeof(int))]
                long Category { get; set; }
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
        Assert.Contains("CategoryId", generated);
        Assert.Contains("string? Name", generated); // Search ToNullable
        // 不 AssertGeneratedCodeCompiles：InterfaceStrs 指向测试源中的标记接口，单独编译生成文件会缺类型。
    }

    [Fact]
    public void Entity_only_mode_omits_other_types()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeClassModes(Id = AutoCodeCrud.Entity, Suffix = "Record")]
            public interface JoinTable
            {
                [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
                long LeftId { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "JoinTable.g.cs");

        Assert.Contains("class JoinTableRecord", generated);
        Assert.DoesNotContain("CreateInput", generated);
        Assert.DoesNotContain("SearchInput", generated);
    }

    [Fact]
    public void Reports_mccj009_for_invalid_inherit_str()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeClassModes(Id = "dto", Suffix = "Dto", InheritStr = "not a type")]
            public interface Broken
            {
                [AutoCodeProperty(Id = "dto")]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        Assert.Contains(result.Diagnostics, d => d.Id == "Mccj009");
    }

    [Fact]
    public void Reports_mccj010_for_invalid_attribute_string()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeClassModes(Id = "dto", Suffix = "Dto", Attributes = new[] { "1NotAnAttribute" })]
            public interface BrokenAttr
            {
                [AutoCodeProperty(Id = "dto")]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        Assert.Contains(result.Diagnostics, d => d.Id == "Mccj010");
    }
}
