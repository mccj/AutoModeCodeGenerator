using AutoCodeGenerator;

namespace Samples.CrudCookbook;

/// <summary>
/// 示例：ModeFlags + Column(Length) + CreateTime mixin + InheritDto 友好写法。
/// 生产环境的 <c>AutoCodeIndex</c> / <c>AutoCodeColumn(BigString)</c> 会生成 SqlSugar 特性字符串，
/// Samples 不引用 SqlSugar，故索引与 BigString 见文档 [CONVENTIONS_TEMPLATE.md]。
/// </summary>
public interface ICreateTimeEntityMarker
{
    System.DateTime? CreateTime { get; set; }
}

public interface IHasCreateTimeMixin
{
    // Create/Update/Detail 继承 Dto：标在 Entity+Dto 即可进三者；Search 单独区间类型。
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto])]
    [AutoCodeProperty(Ids = [AutoCodeCrud.Search], PropertyType = typeof(System.ValueTuple<System.DateTime, System.DateTime>))]
    System.DateTime? CreateTime { get; set; }
}

/// <summary>InjectMarkerMembers 自动投影 CreateTime（无需 IHasCreateTimeMixin）。</summary>
[AutoCodeNullable(Type = NullableEnum.Enable)]
[AutoCodeCrudModes(
    EntityInterfaceTypes = [typeof(ICreateTimeEntityMarker)],
    InjectMarkerMembers = true,
    SearchRangeTemplate = "global::System.ValueTuple<{0},{0}>")]
public interface InboundRawInjected : IHasLongId
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    [AutoCodeColumn(Length = 64)]
    string ProductId { get; set; }
}

[AutoCodeNullable(Type = NullableEnum.Enable)]
[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(ICreateTimeEntityMarker)])]
public interface InboundRawSample : IHasLongId, IHasCreateTimeMixin
{
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    [AutoCodeColumn(Length = 64)]
    string ProductId { get; set; }

    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDtoSearch)]
    [AutoCodeColumn(Length = 128)]
    string DeviceName { get; set; }

    // Detail 继承 Dto：同类型字段只需 Entity+Dto（用 EntityDtoDetail 会在 Detail 上重复声明）
    [AutoCodeProperty(ModeFlags = AutoCodeModeFlags.EntityDto)]
    [AutoCodeColumn(Max = true)]
    string PlatformWire { get; set; }
}

/// <summary>Entity-only 关联表（OmitModes = EntityOnly）。</summary>
[AutoCodeCrudModes(OmitModes = AutoCodeCrudOmit.EntityOnly)]
public interface MappingSample
{
    [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
    long Id { get; set; }

    [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
    long LeftId { get; set; }

    [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
    long RightId { get; set; }
}
