using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace 自动代码生成.CodeGeneration;

internal static class CodeGenerationHelpers
{
    public static string GetPropertyType(SourceGeneratorPropertyInfo property, SourceGeneratorClassInfo classInfo)
        => property.Type + (property.IsNullable == true || classInfo.ToNullable ? "?" : "");

    public static string BuildPropertyModifiers(SourceGeneratorPropertyInfo property)
        => string.Join(" ", new[]
        {
            property.IsNew == true ? "new" : "",
            ModifierToString(property.Modifier),
            property.IsVirtual == true ? "virtual" : "",
            property.IsOverride == true ? "override" : ""
        }.Where(part => !string.IsNullOrWhiteSpace(part)));

    public static ImmutableArray<string> GetPropertyAttributes(SourceGeneratorPropertyInfo property, SourceGeneratorClassInfo classInfo)
    {
        var inheritAttributes = classInfo.InheritAttribute ? property.InheritAttributes : ImmutableArray<string>.Empty;
        return property.Attributes.Concat(inheritAttributes).ToImmutableArray();
    }

    public static string? LowerFirstLetter(string? value)
    {
        if (value is null)
            return null;
        if (value.Length == 0)
            return value;
        return char.ToLower(value[0]) + value.Substring(1);
    }

    public static string ModifierToString(Accessibility? accessibility)
        => accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => ""
        };

    public static string? GetReferenceTypeDefaultInitializer(string propertyType)
    {
        if (propertyType.EndsWith("?", StringComparison.Ordinal))
            return null;

        if (propertyType.EndsWith("[]", StringComparison.Ordinal))
            return "= []";

        var typeName = propertyType.StartsWith("global::", StringComparison.Ordinal)
            ? propertyType.Substring("global::".Length)
            : propertyType;

        return typeName switch
        {
            "bool" or "byte" or "sbyte" or "char" or "decimal" or "double" or "float" or
            "int" or "uint" or "long" or "ulong" or "short" or "ushort" or
            "System.DateTime" or "System.DateTimeOffset" or "System.Guid" => null,
            _ => "= null!"
        };
    }
}
