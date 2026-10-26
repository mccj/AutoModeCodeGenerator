using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 解析 Roslyn <see cref="AttributeData"/> 命名参数的共享工具。
/// Extractor 与各 Expander 必须走此处，避免四套 GetValue 漂移。
/// </summary>
internal static class AttributeArgumentReader
{
    public static T? GetValue<T>(ImmutableArray<KeyValuePair<string, TypedConstant>>? namedArguments, string key)
    {
        if (namedArguments is null)
            return default;

        return GetValue<T>(namedArguments.Value, key);
    }

    public static T? GetValue<T>(ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments, string key)
    {
        if (namedArguments.IsDefaultOrEmpty)
            return default;

        var typedConstant = namedArguments.FirstOrDefault(f => f.Key == key).Value;
        return GetValue<T>(typedConstant);
    }

    public static T? GetValue<T>(TypedConstant typedConstant)
        => GetValue<T>((TypedConstant?)typedConstant);

    public static T? GetValue<T>(TypedConstant? typedConstant)
    {
        if (!typedConstant.HasValue || typedConstant.Value.IsNull)
            return default;

        var type = typeof(T);
        if (typedConstant.Value.Kind == TypedConstantKind.Type
            && type == typeof(string)
            && typedConstant.Value.Value is ITypeSymbol typeSymbol)
        {
            return (T?)(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) as object);
        }

        if (typedConstant.Value.Kind == TypedConstantKind.Enum)
        {
            // Allow reading flags/enums as int / int? (e.g. ModeFlags, OmitModes, Morph Kind).
            if (type == typeof(int) || type == typeof(int?))
                return (T?)(object)Convert.ToInt32(typedConstant.Value.Value!);

            var enumType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                ? type.GenericTypeArguments.FirstOrDefault()
                : type;
            if (enumType is null || !enumType.IsEnum)
                return (T?)(object)Convert.ToInt32(typedConstant.Value.Value!);

            var value = Enum.ToObject(enumType, typedConstant.Value.Value!);
            return (T)value;
        }

        if (typedConstant.Value.Kind != TypedConstantKind.Primitive)
            throw new InvalidOperationException($"不支持的特性参数类型: {typedConstant.Value.Kind}");

        return (T?)typedConstant.Value.Value;
    }

    public static T?[]? GetValues<T>(
        ImmutableArray<KeyValuePair<string, TypedConstant>>? namedArguments,
        string key,
        bool filterNull = true)
    {
        if (namedArguments is null)
            return default;

        return GetValues<T>(namedArguments.Value, key, filterNull);
    }

    public static T?[]? GetValues<T>(
        ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments,
        string key,
        bool filterNull = true)
    {
        if (namedArguments.IsDefaultOrEmpty)
            return default;

        var typedConstant = namedArguments.FirstOrDefault(f => f.Key == key).Value;
        if (typedConstant.IsNull)
            return default;

        if (typedConstant.Kind != TypedConstantKind.Array)
            throw new InvalidOperationException("特性参数不是数组类型");

        return typedConstant.Values
            .Select(static v => GetValue<T>(v))
            .Where(v => !filterNull || v is not null)
            .ToArray();
    }

    /// <summary>宽松读取数组：非数组或缺省时返回 empty，不抛（供 Column/Index 等可选参数）。</summary>
    public static T?[]? TryGetValues<T>(
        ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments,
        string key)
    {
        if (namedArguments.IsDefaultOrEmpty)
            return default;

        var typedConstant = namedArguments.FirstOrDefault(f => f.Key == key).Value;
        if (typedConstant.IsNull || typedConstant.Kind != TypedConstantKind.Array)
            return default;

        return typedConstant.Values
            .Select(static v => GetValueSoft<T>(v))
            .Where(v => v is not null)
            .ToArray();
    }

    /// <summary>宽松读取标量：不支持的 Kind 返回 default，不抛。</summary>
    public static T? TryGetValue<T>(ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments, string key)
    {
        if (namedArguments.IsDefaultOrEmpty)
            return default;

        var typedConstant = namedArguments.FirstOrDefault(f => f.Key == key).Value;
        return GetValueSoft<T>(typedConstant);
    }

    public static ImmutableArray<string> ToImmutableStringArray(IEnumerable<string?>? values)
        => ImmutableArray.CreateRange(
            (values ?? [])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!));

    public static string StripGlobal(string type)
        => type.StartsWith("global::", StringComparison.Ordinal)
            ? type.Substring("global::".Length)
            : type;

    public static ImmutableArray<INamedTypeSymbol> GetTypeSymbols(
        ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments,
        string key)
    {
        if (namedArguments.IsDefaultOrEmpty)
            return ImmutableArray<INamedTypeSymbol>.Empty;

        var typedConstant = namedArguments.FirstOrDefault(f => f.Key == key).Value;
        if (typedConstant.IsNull || typedConstant.Kind != TypedConstantKind.Array)
            return ImmutableArray<INamedTypeSymbol>.Empty;

        return typedConstant.Values
            .Where(v => v.Kind == TypedConstantKind.Type && v.Value is INamedTypeSymbol)
            .Select(v => (INamedTypeSymbol)v.Value!)
            .ToImmutableArray();
    }

    /// <summary>读取单个 <c>typeof(...)</c> 命名参数。</summary>
    public static ITypeSymbol? GetTypeSymbol(
        ImmutableArray<KeyValuePair<string, TypedConstant>>? namedArguments,
        string key)
    {
        if (namedArguments is null || namedArguments.Value.IsDefaultOrEmpty)
            return null;

        var typedConstant = namedArguments.Value.FirstOrDefault(f => f.Key == key).Value;
        if (typedConstant.IsNull || typedConstant.Kind != TypedConstantKind.Type)
            return null;

        return typedConstant.Value as ITypeSymbol;
    }

    /// <summary>
    /// 将开泛型类型（如 <c>typeof(SearchRangeForStructValue&lt;&gt;)</c>）转为
    /// <c>global::…SearchRangeForStructValue&lt;{0}&gt;</c> 模板；非泛型则返回完全限定名。
    /// </summary>
    public static string? FormatTypeAsSearchRangeTemplate(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
            return null;

        if (typeSymbol is not INamedTypeSymbol named)
            return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var def = named.IsGenericType || named.IsUnboundGenericType
            ? named.OriginalDefinition
            : named;

        var fq = def.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var lt = fq.IndexOf('<');
        if (lt >= 0)
            fq = fq.Substring(0, lt);

        if (def.Arity <= 0)
            return fq;

        var placeholders = string.Join(",", Enumerable.Range(0, def.Arity).Select(i => "{" + i + "}"));
        return fq + "<" + placeholders + ">";
    }

    /// <summary>
    /// 解析 Search 区间模板：字符串优先，其次 <c>typeof</c> 开泛型，再回落到 <paramref name="fallback"/>。
    /// </summary>
    public static string ResolveSearchRangeTemplate(
        ImmutableArray<KeyValuePair<string, TypedConstant>>? namedArguments,
        string stringKey,
        string typeKey,
        string fallback)
    {
        var fromString = GetValue<string>(namedArguments, stringKey);
        if (!string.IsNullOrWhiteSpace(fromString))
            return fromString!;

        var fromType = FormatTypeAsSearchRangeTemplate(GetTypeSymbol(namedArguments, typeKey));
        if (!string.IsNullOrWhiteSpace(fromType))
            return fromType!;

        return fallback;
    }

    /// <summary>
    /// 将 ModeFlags 位展开为 <see cref="AutoCodeModeIds"/> 字符串列表。
    /// </summary>
    public static ImmutableArray<string> ExpandModeFlags(int flags)
    {
        var ids = ImmutableArray.CreateBuilder<string>();
        if ((flags & AutoCodeModeFlagBits.Entity) != 0) ids.Add(AutoCodeModeIds.Entity);
        if ((flags & AutoCodeModeFlagBits.Dto) != 0) ids.Add(AutoCodeModeIds.Dto);
        if ((flags & AutoCodeModeFlagBits.Create) != 0) ids.Add(AutoCodeModeIds.Create);
        if ((flags & AutoCodeModeFlagBits.Update) != 0) ids.Add(AutoCodeModeIds.Update);
        if ((flags & AutoCodeModeFlagBits.Detail) != 0) ids.Add(AutoCodeModeIds.Detail);
        if ((flags & AutoCodeModeFlagBits.Search) != 0) ids.Add(AutoCodeModeIds.Search);
        return ids.ToImmutable();
    }

    private static T? GetValueSoft<T>(TypedConstant typedConstant)
    {
        if (typedConstant.IsNull)
            return default;

        try
        {
            return GetValue<T>(typedConstant);
        }
        catch (InvalidOperationException)
        {
            return default;
        }
    }
}
