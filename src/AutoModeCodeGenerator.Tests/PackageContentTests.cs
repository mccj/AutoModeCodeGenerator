using System.IO.Compression;

namespace AutoModeCodeGenerator.Tests;

public class PackageContentTests
{
    [Fact]
    public void Nupkg_contains_analyzer_dll_and_system_text_json()
    {
        var analyzersDir = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "AutoModeCodeGenerator.Analyzers", "bin"));

        var nupkg = Directory.EnumerateFiles(analyzersDir, "AutoModeCodeGenerator.Analyzers.*.nupkg", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        Assert.True(nupkg is not null, $"未找到 nupkg，请先构建 Analyzers 项目。搜索目录: {analyzersDir}");

        using var archive = ZipFile.OpenRead(nupkg!);
        var entries = archive.Entries.Select(e => e.FullName.Replace('\\', '/')).ToArray();

        Assert.Contains(entries, e => e.Equals("analyzers/dotnet/cs/AutoModeCodeGenerator.Analyzers.dll", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(entries, e => e.Equals("analyzers/dotnet/cs/System.Text.Json.dll", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(entries, e => e.Equals("analyzers/dotnet/cs/System.Text.Encodings.Web.dll", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(entries, e => e.Equals("build/AutoModeCodeGenerator.Analyzers.props", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(entries, e => e.Equals("content/entity.schema.json", StringComparison.OrdinalIgnoreCase));
    }
}
