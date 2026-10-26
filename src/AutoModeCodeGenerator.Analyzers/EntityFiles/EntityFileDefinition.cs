namespace AutoModeCodeGenerator.Internal.EntityFiles;

/// <summary>
/// <c>.entity.json</c> 反序列化 DTO（与 content/entity.schema.json 对齐）。
/// </summary>
internal sealed class EntityFileDefinition
{
    public string? Namespace { get; set; }

    public string? ClassName { get; set; }

    public string? Nullable { get; set; }

    public string? ClassStyle { get; set; }

    public string? Modifier { get; set; }

    public bool? IsAbstract { get; set; }

    public bool? IsPartial { get; set; }

    public string? Prefix { get; set; }

    public string? Suffix { get; set; }

    public string? Summary { get; set; }

    public string? Remarks { get; set; }

    public string? Example { get; set; }

    public string? Inherit { get; set; }

    public bool? InheritAttribute { get; set; }

    public bool? ToNullable { get; set; }

    public string[]? Usings { get; set; }

    public string[]? Interfaces { get; set; }

    public string[]? Attributes { get; set; }

    /// <summary>多模式定义；为空时使用根级单类配置（向后兼容）。</summary>
    public EntityModeDefinition[]? Modes { get; set; }

    public EntityPropertyDefinition[]? Properties { get; set; }
}

internal sealed class EntityModeDefinition
{
    public string? Id { get; set; }

    public string? Namespace { get; set; }

    public string? ClassName { get; set; }

    public string? ClassStyle { get; set; }

    public string? Modifier { get; set; }

    public bool? IsAbstract { get; set; }

    public bool? IsPartial { get; set; }

    public string? Prefix { get; set; }

    public string? Suffix { get; set; }

    public string? Summary { get; set; }

    public string? Remarks { get; set; }

    public string? Example { get; set; }

    public string? Inherit { get; set; }

    public bool? InheritAttribute { get; set; }

    public bool? ToNullable { get; set; }

    public string[]? Usings { get; set; }

    public string[]? Interfaces { get; set; }

    public string[]? Attributes { get; set; }
}

internal sealed class EntityPropertyDefinition
{
    public string? Name { get; set; }

    public string? Type { get; set; }

    public string? Id { get; set; }

    public string[]? Ids { get; set; }

    public string? Prefix { get; set; }

    public string? Suffix { get; set; }

    public string? Modifier { get; set; }

    public bool? IsNullable { get; set; }

    public bool? IsVirtual { get; set; }

    public bool? IsOverride { get; set; }

    public bool? IsNew { get; set; }

    public bool? IsInit { get; set; }

    public bool? IsRequired { get; set; }

    public string? DefaultValue { get; set; }

    public string? Summary { get; set; }

    public string? Remarks { get; set; }

    public string? Example { get; set; }

    public string[]? Attributes { get; set; }

    public string[]? InheritAttributes { get; set; }
}
