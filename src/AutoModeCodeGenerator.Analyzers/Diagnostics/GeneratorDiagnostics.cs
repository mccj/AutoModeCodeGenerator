using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 源生成器 / 分析器诊断规则定义（Mccj001–Mccj014）。
/// </summary>
internal static class GeneratorDiagnostics
{
    public static readonly DiagnosticDescriptor GenerationFailed = new(
        id: "Mccj001",
        title: "代码生成异常",
        messageFormat: "为 '{0}' 生成代码时出错: {1}",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidPropertyModifier = new(
        id: "Mccj002",
        title: "属性修饰符冲突",
        messageFormat: "类型 '{0}' 的属性 '{1}' 中 IsVirtual、IsOverride、IsNew 只能有一个为 true",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EntityFileGenerationFailed = new(
        id: "Mccj003",
        title: "entity.json 代码生成失败",
        messageFormat: "处理 '{0}' 时出错: {1}",
        category: nameof(AutoFilesGenerator),
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingClassModeId = new(
        id: "Mccj004",
        title: "ClassMode 缺少 Id",
        messageFormat: "类型 '{0}' 上的 AutoCodeClassModes 缺少或为空的 Id，已跳过该模式",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnboundPropertyMode = new(
        id: "Mccj005",
        title: "属性未绑定到有效 ClassMode",
        messageFormat: "类型 '{0}' 的属性 '{1}' 引用的模式 Id '{2}' 不存在",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicatePropertyName = new(
        id: "Mccj006",
        title: "重复的生成属性名",
        messageFormat: "类型 '{0}' 中生成属性名 '{1}' 重复，已跳过后续定义",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EntityPropertyMissingFields = new(
        id: "Mccj007",
        title: "entity.json 属性缺少必填字段",
        messageFormat: "文件 '{0}' 中存在缺少 name 或 type 的属性，已跳过",
        category: nameof(AutoFilesGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PropertyWithoutClassModes = new(
        id: "Mccj008",
        title: "存在 AutoCodeProperty 但无有效 ClassMode",
        messageFormat: "类型 '{0}' 标注了 AutoCodeProperty，但没有任何有效的 AutoCodeClassModes（含有效 Id）",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidInheritStr = new(
        id: "Mccj009",
        title: "InheritStr 无效",
        messageFormat: "类型 '{0}' 的模式 '{1}' 中 InheritStr '{2}' 不是合法类型名",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidAttributeSyntax = new(
        id: "Mccj010",
        title: "Attributes 字符串无法解析",
        messageFormat: "类型 '{0}' 上的特性字符串无法解析: {1}",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InterfaceMemberNotGenerated = new(
        id: "Mccj011",
        title: "InterfaceTypes 成员未生成",
        messageFormat: "类型 '{0}' 的模式 '{1}' 实现接口 '{2}'，但未生成属性 '{3}'。请在规格或 mixin 上为该成员标注 AutoCodeProperty（标记接口不会自动投影属性）",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor SuggestCrudModes = new(
        id: "Mccj012",
        title: "建议改用 AutoCodeCrudModes",
        messageFormat: "类型 '{0}' 手写了标准六模式 ClassModes，可改用 [AutoCodeCrudModes] 减少样板（可用代码修复）",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.Unnecessary);

    public static readonly DiagnosticDescriptor IndexColumnNotGenerated = new(
        id: "Mccj013",
        title: "索引列未出现在 Entity 投影",
        messageFormat: "类型 '{0}' 的索引 '{1}' 引用列 '{2}'，但 Entity 模式未生成同名属性",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RelationMissingTargetName = new(
        id: "Mccj014",
        title: "AutoCodeRelation 缺少 TargetName",
        messageFormat: "类型 '{0}' 的属性 '{1}' 上 AutoCodeRelation 的 TargetName 为空，已跳过关系展开",
        category: nameof(AutoCodeGenerator),
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, params object?[] messageArgs)
        => Diagnostic.Create(descriptor, location, messageArgs);
}
