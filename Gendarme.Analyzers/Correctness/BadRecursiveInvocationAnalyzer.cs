using Microsoft.CodeAnalysis.Text;

namespace Gendarme.Analyzers.Correctness;

/// <summary>
/// This rule checks for a few common scenarios where a method may be infinitely recursive.
/// For example, getter properties which call themselves or methods with no conditional code which call themselves (instead of the base method).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BadRecursiveInvocationAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.BadRecursiveInvocation_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.BadRecursiveInvocation_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.BadRecursiveInvocation_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.BadRecursiveInvocation,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (methodDeclaration.Body is null && methodDeclaration.ExpressionBody is null)
        {
            return;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
        if (methodSymbol is null)
        {
            return;
        }

        foreach (var invocation in EnumerateInvocations(methodDeclaration))
        {
            if (IsSelfInvocation(invocation, methodSymbol, context.SemanticModel, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name));
            }
        }
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken);
        if (propertySymbol is null)
        {
            return;
        }

        if (propertyDeclaration.ExpressionBody is { } propertyExpressionBody)
        {
            ReportRecursivePropertyReferences(propertyExpressionBody.Expression, propertySymbol, context);
        }

        if (propertyDeclaration.AccessorList is not { } accessorList)
        {
            return;
        }

        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.Body is { } body)
            {
                ReportRecursivePropertyReferences(body, propertySymbol, context);
            }

            if (accessor.ExpressionBody is { } expressionBody)
            {
                ReportRecursivePropertyReferences(expressionBody.Expression, propertySymbol, context);
            }
        }
    }

    private static IEnumerable<InvocationExpressionSyntax> EnumerateInvocations(MethodDeclarationSyntax methodDeclaration)
    {
        if (methodDeclaration.Body is { } body)
        {
            foreach (var invocation in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                yield return invocation;
            }
        }

        if (methodDeclaration.ExpressionBody is { } expressionBody)
        {
            foreach (var invocation in expressionBody.Expression.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
            {
                yield return invocation;
            }
        }
    }

    private static bool IsSelfInvocation(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
        var symbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
        if (symbol is null)
        {
            return false;
        }

        var target = symbol.ReducedFrom ?? symbol;
        return SymbolEqualityComparer.Default.Equals(target.OriginalDefinition, methodSymbol.OriginalDefinition);
    }

    private static void ReportRecursivePropertyReferences(SyntaxNode root, IPropertySymbol propertySymbol, SyntaxNodeAnalysisContext context)
    {
        if (root is null)
        {
            return;
        }

        var reportedSpans = new HashSet<TextSpan>();
        foreach (var node in root.DescendantNodesAndSelf())
        {
            if (!IsPotentialPropertyReferenceNode(node))
            {
                continue;
            }

            if (!IsSelfPropertyReference(node, propertySymbol, context.SemanticModel, context.CancellationToken))
            {
                continue;
            }

            if (reportedSpans.Add(node.Span))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), propertySymbol.Name));
            }
        }
    }

    private static bool IsPotentialPropertyReferenceNode(SyntaxNode node)
    {
        return node is IdentifierNameSyntax or MemberAccessExpressionSyntax;
    }

    private static bool IsSelfPropertyReference(SyntaxNode node, IPropertySymbol propertySymbol, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node is IdentifierNameSyntax identifier)
        {
            if (identifier.Parent is MemberAccessExpressionSyntax memberAccess && memberAccess.Name == identifier)
            {
                return false;
            }

            if (IsPartOfNameof(identifier))
            {
                return false;
            }

            var symbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol;
            return SymbolEqualityComparer.Default.Equals(symbol, propertySymbol);
        }

        if (node is MemberAccessExpressionSyntax memberAccessExpression)
        {
            if (IsPartOfNameof(memberAccessExpression))
            {
                return false;
            }

            var symbol = semanticModel.GetSymbolInfo(memberAccessExpression, cancellationToken).Symbol;
            return SymbolEqualityComparer.Default.Equals(symbol, propertySymbol);
        }

        return false;
    }

    private static bool IsPartOfNameof(SyntaxNode node)
    {
        for (var current = node; current != null; current = current.Parent)
        {
            if (current is InvocationExpressionSyntax invocation
                && invocation.Expression is IdentifierNameSyntax { Identifier.Text: "nameof" })
            {
                return true;
            }
        }

        return false;
    }
}
