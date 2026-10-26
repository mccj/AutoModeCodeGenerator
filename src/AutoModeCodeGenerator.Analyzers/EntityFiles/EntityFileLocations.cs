using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoModeCodeGenerator.Internal.EntityFiles;

/// <summary>
/// 为 entity.json 诊断构造基于文件路径 / 行列的 <see cref="Location"/>。
/// </summary>
internal static class EntityFileLocations
{
    public static Location Create(string filePath, SourceText sourceText, int line, int column)
    {
        if (string.IsNullOrEmpty(filePath))
            return Location.None;

        var path = filePath;
        line = Math.Max(0, line);
        column = Math.Max(0, column);

        if (sourceText.Lines.Count == 0)
        {
            return Location.Create(
                path,
                new TextSpan(0, 0),
                new LinePositionSpan(new LinePosition(0, 0), new LinePosition(0, 0)));
        }

        var lineIndex = Math.Min(line, sourceText.Lines.Count - 1);
        var textLine = sourceText.Lines[lineIndex];
        var start = Math.Min(textLine.Start + column, sourceText.Length);
        var length = Math.Min(1, sourceText.Length - start);

        return Location.Create(
            path,
            new TextSpan(start, length),
            new LinePositionSpan(new LinePosition(lineIndex, column), new LinePosition(lineIndex, column + length)));
    }

    public static Location TryFindPropertyName(string filePath, SourceText sourceText, string propertyName)
    {
        if (string.IsNullOrEmpty(filePath))
            return Location.None;

        var path = filePath;
        var content = sourceText.ToString();
        var marker = $"\"name\": \"{propertyName}\"";
        var index = content.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return Create(path, sourceText, 0, 0);

        var line = sourceText.Lines.GetLineFromPosition(index);
        var column = index - line.Start;
        return Create(path, sourceText, line.LineNumber, column);
    }
}

internal sealed class EntityFileParseException : Exception
{
    public EntityFileParseException(string message, int line, int column)
        : base(message)
    {
        Line = line;
        Column = column;
    }

    public int Line { get; }

    public int Column { get; }
}
