namespace AutoModeCodeGenerator.Conventions.SqlSugar;

/// <summary>
/// 文档用 mixin 说明。请复制到业务程序集（本包不嵌入分析器特性）。
/// 完整模板见仓库 docs/CONVENTIONS_TEMPLATE.md。
/// </summary>
public static class MixinSnippets
{
    /// <summary>
    /// 主键：Entity 有 Id；Update/Detail 也需要；Create/Dto/Search 通常不要。
    /// </summary>
    public const string HasLongIdExample = """
        // public interface IHasLongId
        // {
        //     [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
        //     [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Update, AutoCodeCrud.Detail })]
        //     long Id { get; set; }
        // }
        """;

    /// <summary>
    /// 与运行时 ICreateTimeEntity&lt;T&gt;.CreateTime 对齐的审计字段（含 SearchRange）。
    /// InterfaceTypes 挂 ICreateTimeEntity 时必须同时投影 CreateTime，否则触发 Mccj011。
    /// </summary>
    public const string HasCreateTimeExample = """
        // public interface IHasCreateTime
        // {
        //     [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Detail })]
        //     [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Search }, PropertyTypeStr = "SearchRangeForStructValue<DateTime>")]
        //     DateTime? CreateTime { get; set; }
        // }
        //
        // // 规格：
        // [AutoCodeCrudModes(EntityInterfaceTypes = new[] { typeof(ICreateTimeEntity<long>) })]
        // public interface DeviceBootLogs : IHasLongId, IHasCreateTime { ... }
        """;

    /// <summary>
    /// Admin.NET 风格 CreatedTime / UpdatedTime（与 CreateTime 命名不同，勿混用）。
    /// </summary>
    public const string HasCreatedTimeAuditExample = """
        // public interface IHasCreatedTimeAudit
        // {
        //     [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Detail })]
        //     [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Search }, PropertyTypeStr = "SearchRangeForStructValue<DateTime>")]
        //     DateTime? CreatedTime { get; set; }
        //
        //     [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Entity, AutoCodeCrud.Detail })]
        //     [AutoCodeProperty(Ids = new[] { AutoCodeCrud.Search }, PropertyTypeStr = "SearchRangeForStructValue<DateTime>")]
        //     DateTime? UpdatedTime { get; set; }
        // }
        """;

    /// <summary>Entity-only 关联表（OmitModes = EntityOnly）。</summary>
    public const string EntityOnlyMappingExample = """
        // [AutoCodeCrudModes(OmitModes = AutoCodeCrudOmit.EntityOnly)]
        // public interface ProductConfigFirmwareVersionMapping : IHasLongId
        // {
        //     [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
        //     long ProductConfigId { get; set; }
        //     [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
        //     long FirmwareVersionId { get; set; }
        // }
        """;
}
