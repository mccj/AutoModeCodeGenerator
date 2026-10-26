using AutoCodeGenerator;

namespace Samples.CrudCookbook;

/// <summary>
/// 生产主路径示例：<see cref="AutoCodeCrudModesAttribute"/> + <see cref="AutoCodeRelationAttribute"/>。
/// 等价于旧版六行 <c>AutoCodeClassModes</c> + 外键四段式。
/// </summary>
public interface IEntityMarker
{
}

public interface IHasLongId
{
    [AutoCodeProperty(Id = AutoCodeCrud.Entity)]
    [AutoCodeProperty(Ids = [AutoCodeCrud.Update, AutoCodeCrud.Detail])]
    long Id { get; set; }
}

[AutoCodeNullable(Type = NullableEnum.Enable)]
[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(IEntityMarker)])]
public interface Category : IHasLongId
{
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search])]
    string Name { get; set; }
}

[AutoCodeNullable(Type = NullableEnum.Enable)]
[AutoCodeCrudModes(EntityInterfaceTypes = [typeof(IEntityMarker)])]
public interface Product : IHasLongId
{
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto, AutoCodeCrud.Search])]
    string Name { get; set; }

    /// <summary>
    /// 外键：展开为 CategoryId / Detail / Search /（可选）Navigate。
    /// Samples 关闭 Navigate，避免依赖 SqlSugar；生产可设 IncludeNavigate = true。
    /// </summary>
    [AutoCodeRelation(TargetName = nameof(Category), IncludeNavigate = false)]
    long Category { get; set; }

    /// <summary>Search 区间形态（其它模式仍用普通 Property）。</summary>
    [AutoCodeProperty(Ids = [AutoCodeCrud.Entity, AutoCodeCrud.Dto])]
    [AutoCodeMorph(Kind = AutoCodeMorphKind.SearchRange, RangeTypeTemplate = "global::System.ValueTuple<{0},{0}>")]
    System.DateTime ListedAt { get; set; }
}

/// <summary>Morph.File / StringEnum / StringArray 演示（类型名可按项目覆盖）。</summary>
[AutoCodeNullable(Type = NullableEnum.Enable)]
[AutoCodeCrudModes]
public interface PaymentNote
{
    [AutoCodeMorph(Kind = AutoCodeMorphKind.File, CreateUpdateType = "string", DetailType = "string")]
    string Receipt { get; set; }

    [AutoCodeMorph(Kind = AutoCodeMorphKind.StringEnum, EnumType = "string", EntityType = "string")]
    string Status { get; set; }

    [AutoCodeMorph(Kind = AutoCodeMorphKind.StringArray)]
    string Tags { get; set; }
}
