using System.Collections.Immutable;
using System.Text.Json;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoModeCodeGenerator.Internal.EntityFiles;

/// <summary>
/// entity.json 主路径：将 JSON 解析为 <see cref="SourceGeneratorClassInfo"/>，供共享 Emit 使用。
/// </summary>
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
        var fileName = Path.GetFileName(filePath);
        var defaultClassName = GetDefaultClassName(filePath);
        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        var classInfos = ImmutableArray.CreateBuilder<SourceGeneratorClassInfo>();

        var modes = definition.Modes is { Length: > 0 }
            ? definition.Modes
            : [CreateRootMode(definition)];

        var validModes = new List<(EntityModeDefinition Mode, string ResolvedId)>();
        foreach (var mode in modes)
        {
            if (definition.Modes is { Length: > 0 } && string.IsNullOrWhiteSpace(mode.Id))
            {
                diagnostics.Add(GeneratorDiagnostics.Create(
                    GeneratorDiagnostics.MissingClassModeId,
                    sourceText is null ? Location.None : EntityFileLocations.Create(filePath, sourceText, 0, 0),
                    fileName));
                continue;
            }

            validModes.Add((mode, mode.Id ?? "default"));
        }

        var modeIds = new HashSet<string>(validModes.Select(m => m.ResolvedId), StringComparer.Ordinal);

        foreach (var property in definition.Properties ?? [])
        {
            if (string.IsNullOrWhiteSpace(property.Name) || string.IsNullOrWhiteSpace(property.Type))
            {
                diagnostics.Add(GeneratorDiagnostics.Create(
                    GeneratorDiagnostics.EntityPropertyMissingFields,
                    sourceText is null ? Location.None : EntityFileLocations.Create(filePath, sourceText, 0, 0),
                    fileName));
                continue;
            }

            foreach (var modeId in GetPropertyModeIds(property, hasExplicitModes: definition.Modes is { Length: > 0 }))
            {
                if (modeIds.Contains(modeId))
                    continue;

                diagnostics.Add(GeneratorDiagnostics.Create(
                    GeneratorDiagnostics.UnboundPropertyMode,
                    sourceText is null
                        ? Location.None
                        : EntityFileLocations.TryFindPropertyName(filePath, sourceText, property.Name!),
                    fileName,
                    property.Name!,
                    modeId));
            }
        }

        foreach (var (mode, modeId) in validModes)
        {
            var baseClassName = mode.ClassName ?? definition.ClassName ?? defaultClassName;
            var className = (mode.Prefix ?? definition.Prefix ?? string.Empty)
                + baseClassName
                + (mode.Suffix ?? definition.Suffix ?? string.Empty);
            var classStyle = ParseClassStyle(mode.ClassStyle ?? definition.ClassStyle);
            var inherit = mode.Inherit ?? definition.Inherit
                ?? (classStyle == ClassStyleEnum.Inpc
                    ? "global::AutoCodeGenerator.InpcPropertyBindableBaseAbstract"
                    : null);

            var properties = ImmutableArray.CreateBuilder<SourceGeneratorPropertyInfo>();
            var seenPropertyNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var property in definition.Properties ?? [])
            {
                if (string.IsNullOrWhiteSpace(property.Name) || string.IsNullOrWhiteSpace(property.Type))
                    continue;

                if (!PropertyMatchesMode(property, modeId, hasExplicitModes: definition.Modes is { Length: > 0 }))
                    continue;

                var propertyName = (property.Prefix ?? string.Empty) + property.Name + (property.Suffix ?? string.Empty);
                if (!seenPropertyNames.Add(propertyName))
                {
                    diagnostics.Add(GeneratorDiagnostics.Create(
                        GeneratorDiagnostics.DuplicatePropertyName,
                        sourceText is null
                            ? Location.None
                            : EntityFileLocations.TryFindPropertyName(filePath, sourceText, property.Name!),
                        className,
                        propertyName));
                    continue;
                }

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

                var isInit = property.IsInit == true && classStyle != ClassStyleEnum.Inpc;
                var isRequired = property.IsRequired == true && classStyle != ClassStyleEnum.Inpc;

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
                        .Where(attribute => !string.IsNullOrWhiteSpace(attribute))),
                    IsInit: isInit,
                    IsRequired: isRequired));
            }

            classInfos.Add(new SourceGeneratorClassInfo(
                Name: className,
                Modifier: ParseAccessibility(mode.Modifier ?? definition.Modifier),
                Summary: mode.Summary ?? definition.Summary ?? baseClassName,
                Remarks: mode.Remarks ?? definition.Remarks,
                Example: mode.Example ?? definition.Example,
                ClassNamespace: mode.Namespace ?? definition.Namespace ?? "Generated.Entities",
                Usings: ImmutableArray.CreateRange(mode.Usings ?? definition.Usings ?? []),
                Interfaces: ImmutableArray.CreateRange(
                    (mode.Interfaces ?? definition.Interfaces ?? [])
                    .Where(typeName => !string.IsNullOrWhiteSpace(typeName))),
                IsAbstract: mode.IsAbstract ?? definition.IsAbstract ?? false,
                Inherit: inherit,
                Properties: properties.ToImmutable(),
                Attributes: ImmutableArray.CreateRange(
                    (mode.Attributes ?? definition.Attributes ?? [])
                    .Where(attribute => !string.IsNullOrWhiteSpace(attribute))),
                IsPartial: mode.IsPartial ?? definition.IsPartial ?? true,
                InheritAttribute: mode.InheritAttribute ?? definition.InheritAttribute ?? false,
                ToNullable: mode.ToNullable ?? definition.ToNullable ?? false,
                ClassStyle: classStyle));
        }

        return new GenerationResult(
            defaultClassName,
            ParseNullable(definition.Nullable),
            classInfos.ToImmutable(),
            diagnostics.ToImmutable());
    }

    private static EntityModeDefinition CreateRootMode(EntityFileDefinition definition)
        => new()
        {
            Id = "default",
            Namespace = definition.Namespace,
            ClassName = definition.ClassName,
            ClassStyle = definition.ClassStyle,
            Modifier = definition.Modifier,
            IsAbstract = definition.IsAbstract,
            IsPartial = definition.IsPartial,
            Prefix = definition.Prefix,
            Suffix = definition.Suffix,
            Summary = definition.Summary,
            Remarks = definition.Remarks,
            Example = definition.Example,
            Inherit = definition.Inherit,
            InheritAttribute = definition.InheritAttribute,
            ToNullable = definition.ToNullable,
            Usings = definition.Usings,
            Interfaces = definition.Interfaces,
            Attributes = definition.Attributes,
        };

    private static bool PropertyMatchesMode(EntityPropertyDefinition property, string modeId, bool hasExplicitModes)
    {
        if (!hasExplicitModes)
            return true;

        var ids = GetPropertyModeIds(property, hasExplicitModes: true).ToArray();
        if (ids.Length == 0)
            return true;

        return ids.Contains(modeId, StringComparer.Ordinal);
    }

    private static IEnumerable<string> GetPropertyModeIds(EntityPropertyDefinition property, bool hasExplicitModes)
    {
        if (!hasExplicitModes)
            yield break;

        if (!string.IsNullOrWhiteSpace(property.Id))
            yield return property.Id!;

        foreach (var id in property.Ids ?? [])
        {
            if (!string.IsNullOrWhiteSpace(id))
                yield return id;
        }
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
