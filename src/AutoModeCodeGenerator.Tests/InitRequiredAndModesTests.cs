namespace AutoModeCodeGenerator.Tests;

public class InitRequiredAndModesTests
{
    [Fact]
    public void Generates_init_and_required_on_poco_property()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(Id = "dto", Namespace = "App")]
            public interface IProduct
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "dto", IsInit = true, IsRequired = true)]
                string Name { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IProduct.g.cs");

        Assert.Contains("required", generated);
        Assert.Contains("get; init;", generated);
        SourceGeneratorTestHelper.AssertGeneratedCodeCompiles(result, "IProduct.g.cs");
    }

    [Fact]
    public void Generates_multiple_modes_from_entity_json()
    {
        const string entityJson = """
            {
              "nullable": "enable",
              "modes": [
                { "id": "dto", "namespace": "App.Dtos", "className": "Customer", "suffix": "Dto", "classStyle": "Poco" },
                { "id": "rec", "namespace": "App.Records", "className": "Customer", "suffix": "Rec", "classStyle": "Record" }
              ],
              "properties": [
                { "name": "Id", "type": "int", "ids": ["dto", "rec"] },
                { "name": "Name", "type": "string", "id": "dto" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Customer.entity.json", entityJson);

        Assert.Contains("namespace App.Dtos", generated);
        Assert.Contains("class CustomerDto", generated);
        Assert.Contains("namespace App.Records", generated);
        Assert.Contains("record class CustomerRec", generated);
        Assert.Contains("public int Id", generated);
        Assert.Contains("public string Name", generated);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("Customer.entity.json", entityJson);
    }

    [Fact]
    public void Entity_json_supports_isInit_and_isRequired()
    {
        const string entityJson = """
            {
              "namespace": "App",
              "className": "Item",
              "classStyle": "Poco",
              "properties": [
                { "name": "Code", "type": "string", "isInit": true, "isRequired": true }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Item.entity.json", entityJson);

        Assert.Contains("required", generated);
        Assert.Contains("get; init;", generated);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("Item.entity.json", entityJson);
    }

    [Fact]
    public void Inpc_defaults_inherit_when_inherit_type_omitted()
    {
        const string source = """
            #nullable enable

            [AutoCodeGenerator.AutoCodeClassModes(
                Id = "vm",
                Namespace = "App",
                ClassStyle = AutoCodeGenerator.ClassStyleEnum.Inpc)]
            public interface IToggle
            {
                [AutoCodeGenerator.AutoCodeProperty(Id = "vm")]
                bool IsOn { get; set; }
            }
            """;

        var result = SourceGeneratorTestHelper.RunGenerator(source);
        var generated = SourceGeneratorTestHelper.GetGeneratedSource(result, "IToggle.g.cs");

        Assert.Contains("InpcPropertyBindableBaseAbstract", generated);
        SourceGeneratorTestHelper.AssertGeneratedCodeCompiles(result, "IToggle.g.cs");
    }
}
