using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace AutoModeCodeGenerator.Internal;

/// <summary>
/// 校验 <c>InterfaceTypes</c> / <c>EntityInterfaceTypes</c> 所需成员是否已出现在生成属性中。
/// </summary>
internal static class InterfaceContractChecker
{
    public static void ReportMissingMembers(
        INamedTypeSymbol sourceSymbol,
        ImmutableArray<ClassModeInfo> classModes,
        ImmutableArray<SourceGeneratorClassInfo> classInfos,
        Location location,
        ImmutableArray<Diagnostic>.Builder diagnostics)
    {
        foreach (var mode in classModes)
        {
            if (mode.InterfaceSymbols.IsDefaultOrEmpty)
                continue;

            var generated = classInfos.FirstOrDefault(c =>
                string.Equals(c.Name, mode.Prefix + mode.Name + mode.Suffix, StringComparison.Ordinal));
            if (generated is null)
                continue;

            var generatedNames = new HashSet<string>(
                generated.Properties.Select(p => p.Name).Where(n => !string.IsNullOrEmpty(n)).Select(n => n!),
                StringComparer.Ordinal);

            foreach (var iface in mode.InterfaceSymbols)
            {
                foreach (var prop in EnumerateInterfaceProperties(iface))
                {
                    if (generatedNames.Contains(prop.Name))
                        continue;

                    diagnostics.Add(GeneratorDiagnostics.Create(
                        GeneratorDiagnostics.InterfaceMemberNotGenerated,
                        location,
                        sourceSymbol.ToDisplayString(),
                        mode.Id,
                        iface.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                        prop.Name));
                }
            }
        }
    }

    /// <summary>供 Marker 注入与契约检查共用。</summary>
    internal static IEnumerable<IPropertySymbol> EnumerateInterfaceProperties(INamedTypeSymbol iface)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in iface.AllInterfaces.Append(iface))
        {
            foreach (var prop in type.GetMembers().OfType<IPropertySymbol>())
            {
                if (prop.IsIndexer)
                    continue;
                if (!seen.Add(prop.Name))
                    continue;
                yield return prop;
            }
        }
    }
}
