using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;

using static AutoModeCodeGenerator.Internal.AttributeArgumentReader;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 展开 <c>AutoCodeColumn</c> / <c>AutoCodeIndex</c> 为 Entity 特性字符串与属性绑定。
/// </summary>
internal static class ColumnIndexExpander
{
    internal const string AutoCodeColumnAttribute = "AutoCodeGenerator.AutoCodeColumnAttribute";
    internal const string AutoCodeIndexAttribute = "AutoCodeGenerator.AutoCodeIndexAttribute";

    public static void ExpandColumns(
        INamedTypeSymbol symbol,
        ImmutableArray<PropertyBindingInfo>.Builder result)
    {
        foreach (var property in symbol.GetMembers().OfType<IPropertySymbol>()
                     .Concat(symbol.AllInterfaces.SelectMany(i => i.GetMembers().OfType<IPropertySymbol>())))
        {
            foreach (var attribute in property.GetAttributes().Where(
                f => f.AttributeClass?.ToDisplayString() == AutoCodeColumnAttribute))
            {
                var attrs = BuildColumnAttributes(attribute.NamedArguments);
                if (attrs.Length == 0)
                    continue;

                MergeOrAddEntityBinding(result, property, attrs);
            }
        }
    }

    public static ImmutableArray<string> ExpandIndexes(
        INamedTypeSymbol symbol,
        out ImmutableArray<(string IndexName, string Column)> columnRefs)
    {
        var attrs = ImmutableArray.CreateBuilder<string>();
        var refs = ImmutableArray.CreateBuilder<(string, string)>();

        foreach (var attribute in symbol.GetAttributes().Where(
            f => f.AttributeClass?.ToDisplayString() == AutoCodeIndexAttribute))
        {
            if (!TryBuildIndex(attribute, out var attrText, out var name, out var columns))
                continue;

            attrs.Add(attrText);
            foreach (var col in columns)
                refs.Add((name, col));
        }

        columnRefs = refs.ToImmutable();
        return attrs.ToImmutable();
    }

    private static void MergeOrAddEntityBinding(
        ImmutableArray<PropertyBindingInfo>.Builder result,
        IPropertySymbol property,
        ImmutableArray<string> columnAttrs)
    {
        var merged = false;
        for (var i = 0; i < result.Count; i++)
        {
            var binding = result[i];
            if (binding.SourceName != property.Name || !binding.MatchesMode(AutoCodeModeIds.Entity))
                continue;

            result[i] = binding with
            {
                Attributes = binding.Attributes.AddRange(columnAttrs)
            };
            merged = true;
        }

        if (merged)
            return;

        var type = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)?.TrimEnd('?');
        result.Add(new PropertyBindingInfo(
            SourceName: property.Name,
            Name: property.Name,
            Prefix: null,
            Suffix: null,
            Modifier: property.DeclaredAccessibility,
            Attributes: columnAttrs,
            Remarks: null,
            Example: null,
            SummaryPrefix: null,
            SummarySuffix: null,
            Summary: property.Name,
            Id: null,
            Ids: ImmutableArray.Create(AutoCodeModeIds.Entity),
            DefaultValue: null,
            Type: type,
            IsVirtual: null,
            IsOverride: null,
            IsNew: null,
            IsNullable: property.NullableAnnotation == NullableAnnotation.Annotated,
            InheritAttributes: ImmutableArray<string>.Empty,
            IsInit: null,
            IsRequired: null));
    }

    private static ImmutableArray<string> BuildColumnAttributes(
        ImmutableArray<KeyValuePair<string, TypedConstant>> named)
    {
        var list = ImmutableArray.CreateBuilder<string>();
        var bigString = TryGetValue<bool?>(named, "BigString") == true;
        var max = TryGetValue<bool?>(named, "Max") == true;
        var length = TryGetValue<int?>(named, "Length") ?? 0;
        var columnName = TryGetValue<string>(named, "Name");

        if (bigString)
        {
            list.Add("global::SqlSugar.SugarColumn(ColumnDataType = global::SqlSugar.StaticConfig.CodeFirst_BigString)");
        }
        else if (max)
        {
            list.Add("global::System.ComponentModel.DataAnnotations.StringLength(0x7fffffff)");
        }
        else if (length > 0)
        {
            list.Add($"global::System.ComponentModel.DataAnnotations.StringLength({length})");
        }

        if (!string.IsNullOrWhiteSpace(columnName))
            list.Add($"global::SqlSugar.SugarColumn(ColumnName = \"{Escape(columnName!)}\")");

        var extras = TryGetValues<string>(named, "ExtraAttributes");
        if (extras is not null)
        {
            foreach (var extra in extras)
            {
                if (!string.IsNullOrWhiteSpace(extra))
                    list.Add(extra!);
            }
        }

        return list.ToImmutable();
    }

    private static bool TryBuildIndex(
        AttributeData attribute,
        out string attrText,
        out string name,
        out ImmutableArray<string> columns)
    {
        attrText = "";
        name = "";
        columns = ImmutableArray<string>.Empty;

        var named = attribute.NamedArguments;
        name = TryGetValue<string>(named, "Name") ?? "";
        var unique = TryGetValue<bool?>(named, "Unique") == true;
        var columnList = TryGetValues<string>(named, "Columns");
        var orders = GetOrderValues(named, "Orders");
        var spec = TryGetValue<string>(named, "Spec");

        // Constructor: AutoCodeIndex(string name, params string[] columns)
        if (attribute.ConstructorArguments.Length >= 1
            && attribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive
            && attribute.ConstructorArguments[0].Value is string ctorName
            && !string.IsNullOrWhiteSpace(ctorName))
        {
            if (string.IsNullOrWhiteSpace(name))
                name = ctorName;

            if ((columnList is null || columnList.Length == 0)
                && attribute.ConstructorArguments.Length >= 2
                && attribute.ConstructorArguments[1].Kind == TypedConstantKind.Array)
            {
                columnList = attribute.ConstructorArguments[1].Values
                    .Select(v => v.Value as string)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Cast<string>()
                    .ToArray();
            }
        }

        var pairs = ImmutableArray.CreateBuilder<(string Col, bool Desc)>();
        if (columnList is { Length: > 0 })
        {
            for (var i = 0; i < columnList.Length; i++)
            {
                var col = columnList[i];
                if (string.IsNullOrWhiteSpace(col))
                    continue;
                var desc = orders is { Length: > 0 } && i < orders.Length && orders[i] == 1;
                pairs.Add((col!, desc));
            }
        }
        else if (!string.IsNullOrWhiteSpace(spec))
        {
            foreach (var part in spec!.Split([','], StringSplitOptions.RemoveEmptyEntries))
            {
                var piece = part.Trim();
                if (piece.Length == 0)
                    continue;
                var segs = piece.Split([':'], 2, StringSplitOptions.None);
                var col = segs[0].Trim();
                if (col.Length == 0)
                    continue;
                var desc = segs.Length > 1
                    && segs[1].Trim().Equals("Desc", StringComparison.OrdinalIgnoreCase);
                pairs.Add((col, desc));
            }
        }

        if (string.IsNullOrWhiteSpace(name) || pairs.Count == 0)
            return false;

        var sb = new StringBuilder();
        sb.Append("global::SqlSugar.SugarIndex(\"");
        sb.Append(Escape(name));
        sb.Append('"');
        foreach (var (col, desc) in pairs)
        {
            sb.Append(", \"");
            sb.Append(Escape(col));
            sb.Append("\", ");
            sb.Append(desc
                ? "global::SqlSugar.OrderByType.Desc"
                : "global::SqlSugar.OrderByType.Asc");
        }

        if (unique)
            sb.Append(", IsUnique = true");

        sb.Append(')');
        attrText = sb.ToString();
        columns = pairs.Select(p => p.Col).ToImmutableArray();
        return true;
    }

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static int[]? GetOrderValues(
        ImmutableArray<KeyValuePair<string, TypedConstant>> namedArguments,
        string key)
    {
        var typedConstant = namedArguments.FirstOrDefault(f => f.Key == key).Value;
        if (typedConstant.IsNull || typedConstant.Kind != TypedConstantKind.Array)
            return null;

        return typedConstant.Values
            .Select(v =>
            {
                if (v.IsNull)
                    return 0;
                if (v.Value is int i)
                    return i;
                try { return Convert.ToInt32(v.Value); }
                catch { return 0; }
            })
            .ToArray();
    }

}

