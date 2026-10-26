namespace AutoModeCodeGenerator.Conventions.SqlSugar;

/// <summary>
/// SqlSugar 特性字符串模板（不引用 SqlSugar.dll，仅字面量）。
/// 可与主包 <c>AutoCodeGenerator.AutoCodeAttrMacros</c> / <c>AutoCodeColumn</c> / <c>AutoCodeIndex</c> 并用。
/// </summary>
public static class SqlSugarAttrTemplates
{
    public const string PrimaryKeyIdentity =
        "global::SqlSugar.SugarColumn(IsPrimaryKey = true, IsIdentity = true)";

    public const string PrimaryKey =
        "global::SqlSugar.SugarColumn(IsPrimaryKey = true)";

    public const string Ignore =
        "global::SqlSugar.SugarColumn(IsIgnore = true)";

    public const string BigString =
        "global::SqlSugar.SugarColumn(ColumnDataType = global::SqlSugar.StaticConfig.CodeFirst_BigString)";

    public const string JsonColumn =
        "global::SqlSugar.SugarColumn(IsJson = true)";

    public const string EnumColumn =
        "global::SqlSugar.EnumColumn";

    public const string StringLength64 =
        "global::System.ComponentModel.DataAnnotations.StringLength(64)";

    public const string StringLength128 =
        "global::System.ComponentModel.DataAnnotations.StringLength(128)";

    public const string StringLength1024 =
        "global::System.ComponentModel.DataAnnotations.StringLength(1024)";

    public const string StringLengthMax =
        "global::System.ComponentModel.DataAnnotations.StringLength(0x7fffffff)";

    public const string OpenApiStringEnum =
        "global::System.Text.Json.Serialization.JsonConverter(typeof(global::System.Text.Json.Serialization.JsonStringEnumConverter))";

    public static string Table(string tableName)
        => $"global::SqlSugar.SugarTable(\"{tableName}\")";

    public static string Column(string columnName)
        => $"global::SqlSugar.SugarColumn(ColumnName = \"{columnName}\")";

    public static string NavigateManyToOne(string fkPropertyName)
        => $"global::SqlSugar.Navigate(global::SqlSugar.NavigateType.ManyToOne, nameof({fkPropertyName}))";

    public static string NavigateOneToMany(string fkPropertyName)
        => $"global::SqlSugar.Navigate(global::SqlSugar.NavigateType.OneToMany, nameof({fkPropertyName}))";

    /// <summary>
    /// 构建 SugarIndex 字符串。orders 与 columns 对齐，取值 <c>Asc</c>/<c>Desc</c>（大小写不敏感）；缺省 Asc。
    /// </summary>
    /// <summary>
    /// 日期列绑定自定义 DbType 转换器，例如
    /// <c>DateTimeConverter("global::AutoGeneratorModels.DateTimeConverter")</c>。
    /// </summary>
    public static string DateTimeConverter(string converterTypeFullName)
        => $"global::SqlSugar.SugarColumn(SqlParameterDbType = typeof({converterTypeFullName}))";

    public static string Index(string name, string[] columns, string[]? orders = null, bool unique = false)
    {
        var parts = new System.Text.StringBuilder();
        parts.Append("global::SqlSugar.SugarIndex(\"");
        parts.Append(name);
        parts.Append('"');
        for (var i = 0; i < columns.Length; i++)
        {
            var desc = orders is { Length: > 0 }
                && i < orders.Length
                && string.Equals(orders[i], "Desc", System.StringComparison.OrdinalIgnoreCase);
            parts.Append(", \"");
            parts.Append(columns[i]);
            parts.Append("\", ");
            parts.Append(desc
                ? "global::SqlSugar.OrderByType.Desc"
                : "global::SqlSugar.OrderByType.Asc");
        }

        if (unique)
            parts.Append(", IsUnique = true");

        parts.Append(')');
        return parts.ToString();
    }
}
