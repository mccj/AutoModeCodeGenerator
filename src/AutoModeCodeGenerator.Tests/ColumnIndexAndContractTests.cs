using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Tests;

public class ColumnIndexAndContractTests
{
    [Fact]
    public void Reports_mccj011_when_interface_member_not_generated()
    {
        const string source = """
            #nullable enable
            namespace App;

            public interface ICreateTimeEntity<T>
            {
                System.DateTime? CreateTime { get; set; }
                T Id { get; set; }
            }

            [AutoCodeGenerator.AutoCodeCrudModes(EntityInterfaceTypes = new[] { typeof(ICreateTimeEntity<long>) })]
            public interface RawLog
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto" })]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d =>
            d.Id == "Mccj011"
            && d.Severity == DiagnosticSeverity.Warning
            && d.GetMessage().Contains("CreateTime"));
    }

    [Fact]
    public void No_mccj011_when_create_time_projected()
    {
        const string source = """
            #nullable enable
            namespace App;

            public interface ICreateTimeEntity<T>
            {
                System.DateTime? CreateTime { get; set; }
            }

            public interface IHasCreateTime
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto", "DetailOutput" })]
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "SearchInput" }, PropertyTypeStr = "object")]
                System.DateTime? CreateTime { get; set; }
            }

            [AutoCodeGenerator.AutoCodeCrudModes(EntityInterfaceTypes = new[] { typeof(ICreateTimeEntity<long>) })]
            public interface RawLog : IHasCreateTime
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto" })]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.DoesNotContain(diagnostics, d => d.Id == "Mccj011");
        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "RawLog");
        Assert.Contains("CreateTime", generated);
    }

    [Fact]
    public void InheritDto_and_ModeFlags_still_generate()
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

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "Product");
        Assert.Contains("class ProductCreateInput", generated);
        Assert.Contains(": ProductDto", generated);
        Assert.Contains("Title", generated);
    }

    [Fact]
    public void AutoCodeColumn_and_Index_expand_on_entity()
    {
        const string source = """
            #nullable enable
            namespace App;

            [AutoCodeGenerator.AutoCodeCrudModes(
                EntityAttributes = new[] { "global::SqlSugar.SugarTable(\"InboundRaw\")" })]
            [AutoCodeGenerator.AutoCodeIndex("IX_Inbound_CreateTime", Spec = "CreateTime:Desc")]
            [AutoCodeGenerator.AutoCodeIndex(Name = "UK_Device", Unique = true, Columns = new[] { "DeviceName" }, Orders = new[] { AutoCodeGenerator.AutoCodeOrderBy.Asc })]
            public interface InboundRaw
            {
                [AutoCodeGenerator.AutoCodeProperty(ModeFlags = AutoCodeGenerator.AutoCodeModeFlags.EntityDtoSearch)]
                [AutoCodeGenerator.AutoCodeColumn(Length = 64)]
                string? DeviceName { get; set; }

                [AutoCodeGenerator.AutoCodeProperty(ModeFlags = AutoCodeGenerator.AutoCodeModeFlags.EntityDtoDetail)]
                [AutoCodeGenerator.AutoCodeColumn(BigString = true)]
                string? PlatformWire { get; set; }

                [AutoCodeGenerator.AutoCodeProperty(ModeFlags = AutoCodeGenerator.AutoCodeModeFlags.EntityDtoDetailSearch)]
                System.DateTime? CreateTime { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);
        Assert.DoesNotContain(diagnostics, d => d.Id is "Mccj001" or "Mccj013" && d.Severity == DiagnosticSeverity.Error);

        var generated = SourceGeneratorTestHelper.GetGeneratedSourcesContaining(result, "InboundRaw");
        Assert.Contains("StringLength(64)", generated);
        Assert.Contains("CodeFirst_BigString", generated);
        Assert.Contains("SugarIndex(\"IX_Inbound_CreateTime\"", generated);
        Assert.Contains("OrderByType.Desc", generated);
        Assert.Contains("IsUnique = true", generated);
    }

    [Fact]
    public void Reports_mccj013_when_index_column_missing()
    {
        const string source = """
            #nullable enable
            namespace App;

            [AutoCodeGenerator.AutoCodeCrudModes]
            [AutoCodeGenerator.AutoCodeIndex(Name = "IX_Missing", Spec = "NotAField:Asc")]
            public interface Broken
            {
                [AutoCodeGenerator.AutoCodeProperty(Ids = new[] { "Entity", "Dto" })]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var diagnostics = SourceGeneratorTestHelper.GetGeneratorDiagnostics(result);

        Assert.Contains(diagnostics, d =>
            d.Id == "Mccj013"
            && d.GetMessage().Contains("NotAField"));
    }
}
