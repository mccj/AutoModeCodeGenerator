namespace Samples.CrudCookbook;

/// <summary>
/// 历史参考：生产项目常自建 Id/Suffix 常量。
/// 推荐直接使用 <see cref="AutoCodeGenerator.AutoCodeCrud"/>（Entity/Dto/Create/…）。
/// 本文件保留以便对照旧 README / 升级指南中的手写六模式模板。
/// </summary>
internal static class AutoGeneratorConst
{
    public const string EntityId = AutoCodeGenerator.AutoCodeCrud.Entity;
    public const string EntitySuffix = AutoCodeGenerator.AutoCodeCrud.DefaultEntitySuffix;

    public const string DtoId = AutoCodeGenerator.AutoCodeCrud.Dto;
    public const string DtoSuffix = AutoCodeGenerator.AutoCodeCrud.DefaultDtoSuffix;

    public const string CreateId = AutoCodeGenerator.AutoCodeCrud.Create;
    public const string CreateSuffix = AutoCodeGenerator.AutoCodeCrud.DefaultCreateSuffix;
    public const string CreateSummaryPrefix = "创建";

    public const string UpdateId = AutoCodeGenerator.AutoCodeCrud.Update;
    public const string UpdateSuffix = AutoCodeGenerator.AutoCodeCrud.DefaultUpdateSuffix;
    public const string UpdateSummaryPrefix = "更新";

    public const string DetailId = AutoCodeGenerator.AutoCodeCrud.Detail;
    public const string DetailSuffix = AutoCodeGenerator.AutoCodeCrud.DefaultDetailSuffix;
    public const string DetailSummaryPrefix = "详情";

    public const string SearchId = AutoCodeGenerator.AutoCodeCrud.Search;
    public const string SearchSuffix = AutoCodeGenerator.AutoCodeCrud.DefaultSearchSuffix;
    public const string SearchSummaryPrefix = "搜索";
}
