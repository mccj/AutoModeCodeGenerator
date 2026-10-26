namespace AutoModeCodeGenerator.Tests;

public class MorphGenerationTests
{
    [Fact]
    public void File_morph_uses_dto_types_per_mode()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeCrudModes]
            public interface Payment
            {
                [AutoCodeMorph(Kind = AutoCodeMorphKind.File,
                    CreateUpdateType = "UploadFileInput",
                    DetailType = "FileOutput")]
                string Receipt { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Payment.g.cs");

        Assert.Contains("class PaymentRecord", generated);
        Assert.Contains("string Receipt", ExtractClassBody(generated, "PaymentRecord"));
        Assert.Contains("UploadFileInput Receipt", ExtractClassBody(generated, "PaymentDto"));
        Assert.Contains("FileOutput Receipt", ExtractClassBody(generated, "PaymentDetailOutput"));
        // Create/Update 继承 Dto，不重复投影
        Assert.DoesNotContain("Receipt", ExtractClassBody(generated, "PaymentCreateInput"));
        Assert.DoesNotContain("Receipt", ExtractClassBody(generated, "PaymentSearchInput"));
    }

    [Fact]
    public void SearchRange_morph_only_on_search()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeCrudModes]
            public interface Order
            {
                [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Dto })]
                [AutoCodeMorph(Kind = AutoCodeMorphKind.SearchRange)]
                global::System.DateTime Created { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Order.g.cs");

        Assert.Contains("DateTime Created", generated);
        Assert.Contains("SearchRange<", generated);
        Assert.Contains("System.DateTime", ExtractClassBody(generated, "OrderSearchInput"));
    }

    [Fact]
    public void StringEnum_morph_keeps_enum_type_splits_attributes()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            public enum PayStatus { Pending, Paid }

            [AutoCodeCrudModes]
            public interface Bill
            {
                [AutoCodeMorph(
                    Kind = AutoCodeMorphKind.StringEnum,
                    EnumType = nameof(PayStatus),
                    EntityAttributes = new[] { AutoCodeAttrMacros.SugarColumnIgnore },
                    ApiAttributes = new[] { AutoCodeAttrMacros.JsonIgnore })]
                PayStatus Status { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Bill.g.cs");

        Assert.Contains("PayStatus Status", ExtractClassBody(generated, "BillRecord"));
        Assert.Contains("PayStatus Status", ExtractClassBody(generated, "BillDto"));
        Assert.Contains("SqlSugar.SugarColumn(IsIgnore = true)", generated);
        Assert.Contains("JsonIgnore", generated);
    }

    [Fact]
    public void StringEnum_morph_can_force_entity_string_column()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            public enum PayStatus { Pending, Paid }

            [AutoCodeCrudModes]
            public interface Bill
            {
                [AutoCodeMorph(
                    Kind = AutoCodeMorphKind.StringEnum,
                    EntityType = "string",
                    EnumType = nameof(PayStatus))]
                PayStatus Status { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "Bill.g.cs");
        Assert.Contains("string Status", ExtractClassBody(generated, "BillRecord"));
        Assert.Contains("PayStatus Status", ExtractClassBody(generated, "BillDto"));
    }

    [Fact]
    public void StringArray_morph_entity_csv_api_array()
    {
        const string source = """
            #nullable enable
            using AutoCodeGenerator;

            [AutoCodeCrudModes]
            public interface TagHolder
            {
                [AutoCodeMorph(Kind = AutoCodeMorphKind.StringArray)]
                string Tags { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "TagHolder.g.cs");

        Assert.Contains("string Tags", generated);
        Assert.Contains("string[] Tags", generated);
    }

    private static string ExtractClassBody(string source, string className)
    {
        var marker = "class " + className;
        var idx = source.IndexOf(marker, StringComparison.Ordinal);
        if (idx < 0)
            return string.Empty;
        var end = source.IndexOf("class ", idx + marker.Length, StringComparison.Ordinal);
        return end < 0 ? source.Substring(idx) : source.Substring(idx, end - idx);
    }
}
