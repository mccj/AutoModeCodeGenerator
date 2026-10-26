using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using AutoModeCodeGenerator.Internal.EntityFiles;

namespace AutoModeCodeGenerator.Tests;

public class EntityFileGenerationTests
{
    [Fact]
    public void Generates_poco_from_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "Customer",
              "nullable": "enable",
              "properties": [
                { "name": "Id", "type": "int" },
                { "name": "Name", "type": "string", "isNullable": true }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Customer.entity.json", entityJson);

        Assert.Contains("namespace App.Entities", generated);
        Assert.Contains("public partial class Customer", generated);
        Assert.Contains("public int Id { get; set; }", generated);
        Assert.Contains("public string? Name { get; set; }", generated);
    }

    [Fact]
    public void Uses_file_name_when_class_name_is_omitted()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "properties": [
                { "name": "Code", "type": "string" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Order.entity.json", entityJson);

        Assert.Contains("public partial class Order", generated);
    }

    [Fact]
    public void Generated_entity_output_compiles_without_errors()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "Product",
              "nullable": "enable",
              "properties": [
                { "name": "Sku", "type": "string" },
                { "name": "Price", "type": "decimal" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Product.entity.json", entityJson);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("Product.entity.json", entityJson);
    }

    [Fact]
    public void Generates_record_style_from_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "Point",
              "classStyle": "Record",
              "properties": [
                { "name": "X", "type": "double" },
                { "name": "Y", "type": "double" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Point.entity.json", entityJson);

        Assert.Contains("record class Point", generated);
        Assert.Contains("public Point(double @x", generated);
        Assert.Contains("public double X { get; init; }", generated);
        Assert.Contains("public double Y { get; init; }", generated);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("Point.entity.json", entityJson);
    }

    [Fact]
    public void Generates_inpc_style_from_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.ViewModels",
              "className": "CounterVm",
              "classStyle": "Inpc",
              "properties": [
                { "name": "Count", "type": "int" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("CounterVm.entity.json", entityJson);

        Assert.Contains(
            "public partial class CounterVm : global::AutoCodeGenerator.InpcPropertyBindableBaseAbstract",
            generated);
        Assert.Contains("SetValue(value)", generated);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("CounterVm.entity.json", entityJson);
    }

    [Fact]
    public void Supports_to_nullable_on_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "Item",
              "toNullable": true,
              "properties": [
                { "name": "Id", "type": "int" },
                { "name": "Name", "type": "string" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Item.entity.json", entityJson);

        Assert.Contains("public int? Id { get; set; }", generated);
        Assert.Contains("public string? Name { get; set; }", generated);
    }

    [Fact]
    public void Supports_interfaces_and_attributes_on_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "UserDto",
              "attributes": [ "global::System.Serializable" ],
              "interfaces": [ "App.Entities.IEntity" ],
              "properties": [
                {
                  "name": "Id",
                  "type": "int",
                  "attributes": [ "global::System.Obsolete(\"legacy field\")" ]
                }
              ]
            }
            """;

        const string additionalSource = """
            namespace App.Entities
            {
                public interface IEntity
                {
                    int Id { get; set; }
                }
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("UserDto.entity.json", entityJson);

        Assert.Contains("[global::System.Serializable]", generated);
        Assert.Contains(": App.Entities.IEntity", generated);
        Assert.Contains("[global::System.Obsolete(\"legacy field\")]", generated);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("UserDto.entity.json", entityJson, additionalSource);
    }

    [Fact]
    public void Supports_class_modifier_isAbstract_and_isPartial_on_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "EntityBase",
              "modifier": "internal",
              "isAbstract": true,
              "isPartial": false,
              "properties": [
                { "name": "Id", "type": "int" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("EntityBase.entity.json", entityJson);

        Assert.Contains("internal abstract class EntityBase", generated);
        Assert.DoesNotContain("partial class EntityBase", generated);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("EntityBase.entity.json", entityJson);
    }

    [Fact]
    public void Supports_property_modifier_and_defaultValue_on_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "Settings",
              "properties": [
                {
                  "name": "Title",
                  "type": "string",
                  "modifier": "internal",
                  "defaultValue": "= \"Untitled\""
                },
                {
                  "name": "Revision",
                  "type": "int",
                  "isVirtual": true
                }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Settings.entity.json", entityJson);

        Assert.Contains("internal string Title { get; set; } = \"Untitled\";", generated);
        Assert.Contains("public virtual int Revision { get; set; }", generated);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("Settings.entity.json", entityJson);
    }

    [Fact]
    public void Uses_default_namespace_when_omitted_in_entity_json()
    {
        const string entityJson = """
            {
              "properties": [
                { "name": "Code", "type": "string" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Fallback.entity.json", entityJson);

        Assert.Contains("namespace Generated.Entities", generated);
        Assert.Contains("public partial class Fallback", generated);
    }

    [Fact]
    public void Emits_usings_from_entity_json()
    {
        const string entityJson = """
            {
              "namespace": "App.Entities",
              "className": "Tagged",
              "usings": [ "System.Collections.Generic" ],
              "properties": [
                { "name": "Id", "type": "int" }
              ]
            }
            """;

        var generated = SourceGeneratorTestHelper.GenerateFromEntityJson("Tagged.entity.json", entityJson);

        Assert.Contains("using System.Collections.Generic;", generated);
        SourceGeneratorTestHelper.AssertEntityJsonCompiles("Tagged.entity.json", entityJson);
    }

    [Fact]
    public void Invalid_entity_json_throws_parse_exception()
    {
        const string entityJson = "{ invalid json";

        var exception = Assert.Throws<EntityFileParseException>(() => EntityFileParser.Parse(entityJson));
        Assert.NotNull(exception.Message);
    }
}
