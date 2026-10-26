using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using AutoModeCodeGenerator.Internal;

namespace AutoModeCodeGenerator;

/// <summary>
/// 将标准六行 <c>AutoCodeClassModes</c> 替换为 <c>[AutoCodeCrudModes]</c>。
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SuggestCrudModesCodeFixProvider))]
[Shared]
public sealed class SuggestCrudModesCodeFixProvider : CodeFixProvider
{
    private const string Title = "改用 AutoCodeCrudModes";

    public override ImmutableArray<string> FixableDiagnosticIds { get; }
        = ImmutableArray.Create(GeneratorDiagnostics.SuggestCrudModes.Id);

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        var diagnostic = context.Diagnostics.First();
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var typeDecl = node.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (typeDecl is null)
            return;

        context.RegisterCodeFix(
            CodeAction.Create(
                Title,
                ct => ApplyAsync(context.Document, root, typeDecl, ct),
                equivalenceKey: Title),
            diagnostic);
    }

    private static Task<Document> ApplyAsync(
        Document document,
        SyntaxNode root,
        TypeDeclarationSyntax typeDecl,
        CancellationToken cancellationToken)
    {
        _ = cancellationToken;

        string? entityInterfaces = null;
        string? entityAttributes = null;

        var newAttributeLists = new List<AttributeListSyntax>();
        foreach (var list in typeDecl.AttributeLists)
        {
            var kept = new List<AttributeSyntax>();
            foreach (var attr in list.Attributes)
            {
                var name = attr.Name.ToString();
                if (name.Contains("AutoCodeClassModes"))
                {
                    var id = TryGetNamedString(attr, "Id");
                    if (id == AutoCodeModeIds.Entity)
                    {
                        entityInterfaces = TryGetTypeOfArray(attr, "InterfaceTypes")
                            ?? TryGetNamedStringArrayExpr(attr, "InterfaceStrs");
                        entityAttributes = TryGetNamedStringArrayExpr(attr, "Attributes");
                    }

                    continue; // drop ClassModes
                }

                kept.Add(attr);
            }

            if (kept.Count > 0)
            {
                newAttributeLists.Add(list.WithAttributes(SyntaxFactory.SeparatedList(kept)));
            }
        }

        var crudArgs = new List<AttributeArgumentSyntax>();
        if (!string.IsNullOrWhiteSpace(entityInterfaces))
        {
            crudArgs.Add(SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals("EntityInterfaceTypes"),
                null,
                SyntaxFactory.ParseExpression(entityInterfaces!)));
        }

        if (!string.IsNullOrWhiteSpace(entityAttributes))
        {
            crudArgs.Add(SyntaxFactory.AttributeArgument(
                SyntaxFactory.NameEquals("EntityAttributes"),
                null,
                SyntaxFactory.ParseExpression(entityAttributes!)));
        }

        var crudAttr = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("AutoCodeGenerator.AutoCodeCrudModes"),
            crudArgs.Count == 0
                ? null
                : SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(crudArgs)));

        newAttributeLists.Insert(0, SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(crudAttr)));

        var newType = typeDecl.WithAttributeLists(SyntaxFactory.List(newAttributeLists));
        var newRoot = root.ReplaceNode(typeDecl, newType);
        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }

    private static string? TryGetNamedString(AttributeSyntax attr, string name)
    {
        var arg = attr.ArgumentList?.Arguments
            .FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == name);
        if (arg?.Expression is LiteralExpressionSyntax lit
            && lit.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return lit.Token.ValueText;
        }

        return null;
    }

    private static string? TryGetTypeOfArray(AttributeSyntax attr, string name)
    {
        var arg = attr.ArgumentList?.Arguments
            .FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == name);
        return arg?.Expression.ToString();
    }

    private static string? TryGetNamedStringArrayExpr(AttributeSyntax attr, string name)
    {
        var arg = attr.ArgumentList?.Arguments
            .FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == name);
        return arg?.Expression.ToString();
    }
}
