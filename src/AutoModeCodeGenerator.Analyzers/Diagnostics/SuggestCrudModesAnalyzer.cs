using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using AutoModeCodeGenerator.Internal;

namespace AutoModeCodeGenerator;

/// <summary>
/// 手写标准六模式 ClassModes 时报告 Mccj012（可由 CodeFix 替换为 AutoCodeCrudModes）。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SuggestCrudModesAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] RequiredIds =
    [
        AutoCodeModeIds.Entity,
        AutoCodeModeIds.Dto,
        AutoCodeModeIds.Create,
        AutoCodeModeIds.Update,
        AutoCodeModeIds.Detail,
        AutoCodeModeIds.Search
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(GeneratorDiagnostics.SuggestCrudModes);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeType, SyntaxKind.InterfaceDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeType(SyntaxNodeAnalysisContext context)
    {
        var typeDecl = (TypeDeclarationSyntax)context.Node;
        if (typeDecl.AttributeLists.Count == 0)
            return;

        var hasCrudModes = false;
        var classModeIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var list in typeDecl.AttributeLists)
        {
            foreach (var attr in list.Attributes)
            {
                var name = attr.Name.ToString();
                if (name.Contains("AutoCodeCrudModes"))
                {
                    hasCrudModes = true;
                    break;
                }

                if (!name.Contains("AutoCodeClassModes"))
                    continue;

                var id = TryGetIdArgument(attr);
                if (!string.IsNullOrWhiteSpace(id))
                    classModeIds.Add(id!);
            }

            if (hasCrudModes)
                break;
        }

        if (hasCrudModes)
            return;

        if (RequiredIds.Any(id => !classModeIds.Contains(id)))
            return;

        if (classModeIds.Count != RequiredIds.Length)
            return;

        var typeName = context.SemanticModel.GetDeclaredSymbol(typeDecl, context.CancellationToken)?.ToDisplayString()
            ?? typeDecl.Identifier.Text;

        context.ReportDiagnostic(Diagnostic.Create(
            GeneratorDiagnostics.SuggestCrudModes,
            typeDecl.Identifier.GetLocation(),
            typeName));
    }

    private static string? TryGetIdArgument(AttributeSyntax attr)
    {
        if (attr.ArgumentList is null)
            return null;

        foreach (var arg in attr.ArgumentList.Arguments)
        {
            if (arg.NameEquals?.Name.Identifier.Text != "Id")
                continue;

            if (arg.Expression is LiteralExpressionSyntax lit
                && lit.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return lit.Token.ValueText;
            }
        }

        return null;
    }
}
