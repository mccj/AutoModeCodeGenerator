using System.Collections.Immutable;
using System.Text.Json;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace 自动代码生成.EntityFiles;

internal static class EntityFileParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static EntityFileDefinition Parse(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<EntityFileDefinition>(json, JsonOptions)
                ?? throw new EntityFileParseException("entity.json 内容为空或格式无效", 0, 0);
        }
        catch (JsonException ex)
        {
            throw new EntityFileParseException(ex.Message, (int)(ex.LineNumber ?? 0), (int)(ex.BytePositionInLine ?? 0));
        }
    }

    public static string GetDefaultClassName(string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        const string entitySuffix = ".entity";
        if (fileName.EndsWith(entitySuffix, StringComparison.OrdinalIgnoreCase))
            fileName = fileName.Substring(0, fileName.Length - entitySuffix.Length);

        return string.IsNullOrWhiteSpace(fileName) ? "Entity" : fileName;
    }

    public static GenerationResult ToGenerationResult(
        string filePath,
        EntityFileDefinition definition,
        SourceText? sourceText = null)
    {
        var baseClassName = definition.ClassName ?? GetDefaultClassName(filePath);
        var className = (definition.Prefix ?? string.Empty) + baseClassName + (definition.Suffix ?? string.Empty);
        var nullable = ParseNullable(definition.Nullable);
        var classStyle = ParseClassStyle(definition.ClassStyle);
        var inherit = definition.Inherit ?? (classStyle == ClassStyleEnum.Inpc
            ? "global::AutoCodeGenerator.InpcPropertyBindableBaseAbstract"
            : null);
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var properties = ImmutableArray.CreateBuilder<SourceGeneratorPropertyInfo>();

        foreach (var property in definition.Properties ?? [])
        {
            if (string.IsNullOrWhiteSpace(property.Name) || string.IsNullOrWhiteSpace(property.Type))
                continue;

            var propertyName = (property.Prefix ?? string.Empty) + property.Name + (property.Suffix ?? string.Empty);
            if (CountTrue(property.IsVirtual, property.IsOverride, property.IsNew) > 1)
            {
                var location = sourceText is null
                    ? Location.None
                    : EntityFileLocations.TryFindPropertyName(filePath, sourceText, property.Name!);
                diagnostics.Add(GeneratorDiagnostics.Create(
                    GeneratorDiagnostics.InvalidPropertyModifier,
                    location,
                    className,
                    propertyName));
                continue;
            }

            properties.Add(new SourceGeneratorPropertyInfo(
                Name: propertyName,
                Modifier: ParseAccessibility(property.Modifier),
                Summary: property.Summary ?? property.Name,
                Remarks: property.Remarks,
                Example: property.Example,
                IsVirtual: property.IsVirtual,
                IsOverride: property.IsOverride,
                IsNew: property.IsNew,
                Type: property.Type,
                DefaultValue: property.DefaultValue,
                IsNullable: property.IsNullable,
                Attributes: ImmutableArray.CreateRange(
                    (property.Attributes ?? [])
                    .Where(attribute => !string.IsNullOrWhiteSpace(attribute))),
                InheritAttributes: ImmutableArray.CreateRange(
                    (property.InheritAttributes ?? [])
                    .Where(attribute => !string.IsNullOrWhiteSpace(attribute)))));
        }

        var classInfo = new SourceGeneratorClassInfo(
            Name: className,
            Modifier: ParseAccessibility(definition.Modifier),
            Summary: definition.Summary ?? baseClassName,
            Remarks: definition.Remarks,
            Example: definition.Example,
            ClassNamespace: definition.Namespace ?? "Generated.Entities",
            Usings: ImmutableArray.CreateRange(definition.Usings ?? []),
            Interfaces: ImmutableArray.CreateRange(
                (definition.Interfaces ?? [])
                .Where(typeName => !string.IsNullOrWhiteSpace(typeName))),
            IsAbstract: definition.IsAbstract ?? false,
            Inherit: inherit,
            Properties: properties.ToImmutable(),
            Attributes: ImmutableArray.CreateRange(
                (definition.Attributes ?? [])
                .Where(attribute => !string.IsNullOrWhiteSpace(attribute))),
            IsPartial: definition.IsPartial ?? true,
            InheritAttribute: definition.InheritAttribute ?? false,
            ToNullable: definition.ToNullable ?? false,
            ClassStyle: classStyle);

        return new GenerationResult(
            baseClassName,
            nullable,
            ImmutableArray.Create(classInfo),
            diagnostics.ToImmutable());
    }

    private static int CountTrue(params bool?[] values)
        => values.Count(value => value == true);

    private static Accessibility ParseAccessibility(string? modifier)
        => modifier?.Trim().ToLowerInvariant() switch
        {
            null or "" => Accessibility.Public,
            "public" => Accessibility.Public,
            "internal" => Accessibility.Internal,
            "protected" => Accessibility.Protected,
            "private" => Accessibility.Private,
            "protected internal" or "protectedinternal" => Accessibility.ProtectedOrInternal,
            "private protected" or "privateprotected" => Accessibility.ProtectedAndInternal,
            _ => throw new InvalidOperationException($"不支持的 modifier 值: {modifier}")
        };

    private static ClassStyleEnum ParseClassStyle(string? classStyle)
        => classStyle?.Trim().ToLowerInvariant() switch
        {
            null or "" or "poco" => ClassStyleEnum.Poco,
            "record" => ClassStyleEnum.Record,
            "inpc" => ClassStyleEnum.Inpc,
            _ => throw new InvalidOperationException($"不支持的 classStyle 值: {classStyle}")
        };

    private static NullableEnum? ParseNullable(string? nullable)
        => nullable?.Trim().ToLowerInvariant() switch
        {
            null or "" => NullableEnum.Enable,
            "enable" => NullableEnum.Enable,
            "disable" => NullableEnum.Disable,
            "restore" => NullableEnum.Restore,
            "enable warnings" => NullableEnum.EnableWarnings,
            "disable warnings" => NullableEnum.DisableWarnings,
            "restore warnings" => NullableEnum.RestoreWarnings,
            "enable annotations" => NullableEnum.EnableAnnotations,
            "disable annotations" => NullableEnum.DisableAnnotations,
            "restore annotations" => NullableEnum.RestoreAnnotations,
            _ => throw new InvalidOperationException($"不支持的 nullable 值: {nullable}")
        };
}
