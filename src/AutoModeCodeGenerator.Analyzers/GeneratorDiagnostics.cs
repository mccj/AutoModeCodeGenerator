using Microsoft.CodeAnalysis;

namespace 自动代码生成;

/// <summary>
/// 源生成器诊断规则定义（Mccj001–Mccj003）。
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

    public static Diagnostic Create(DiagnosticDescriptor descriptor, Location location, params object?[] messageArgs)
        => Diagnostic.Create(descriptor, location, messageArgs);
}
