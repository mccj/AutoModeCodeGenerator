using System.Text;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 读取打包在程序集中的嵌入模板（特性定义等），使用 UTF-8 解码以避免中文乱码。
/// </summary>
internal static class EmbeddedTemplates
{
    internal const string AutoCodeAttributesResourceName = "AutoModeCodeGenerator.Analyzers.Templates.AutoCodeAttributes.cs";

    public static string ReadAutoCodeAttributes()
    {
        var assembly = typeof(EmbeddedTemplates).Assembly;
        using var stream = assembly.GetManifestResourceStream(AutoCodeAttributesResourceName)
            ?? throw new InvalidOperationException($"找不到嵌入资源: {AutoCodeAttributesResourceName}");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
