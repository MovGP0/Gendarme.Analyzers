namespace Gendarme.Analyzers.Correctness;

/// <summary>
/// This rule checks for methods that do not require anything from the current instance.
/// Those methods can be converted into static methods, which helps a bit with performance (the hidden this parameter can be omitted), and clarifies the API.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodCanBeMadeStaticAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MethodCanBeMadeStatic_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MethodCanBeMadeStatic_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MethodCanBeMadeStatic_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MethodCanBeMadeStatic,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
            || methodDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword)
            || methodDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
        {
            return;
        }

        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken);
        if (methodSymbol is null)
        {
            return;
        }

        if (methodSymbol.MethodKind is not MethodKind.Ordinary
            || methodSymbol.IsStatic
            || methodSymbol.IsVirtual
            || methodSymbol.IsOverride)
        {
            return;
        }

        if (methodSymbol.ContainingType?.TypeKind != TypeKind.Class)
        {
            return;
        }

        if (methodDeclaration.Body is null && methodDeclaration.ExpressionBody is null)
        {
            return;
        }

        if (UsesInstanceState(methodDeclaration, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodSymbol.Name));
    }

    private static bool UsesInstanceState(MethodDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        foreach (var node in EnumerateRelevantNodes(declaration))
        {
            if (node is ThisExpressionSyntax or BaseExpressionSyntax)
            {
                return true;
            }

            if (IsPartOfNameof(node))
            {
                continue;
            }

            var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
            var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

            if (RequiresInstanceContext(symbol))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<SyntaxNode> EnumerateRelevantNodes(MethodDeclarationSyntax declaration)
    {
        if (declaration.Body is { } body)
        {
            foreach (var node in body.DescendantNodes().Where(IsRelevantNode))
            {
                yield return node;
            }
        }

        if (declaration.ExpressionBody is { } expressionBody)
        {
            foreach (var node in expressionBody.Expression.DescendantNodesAndSelf().Where(IsRelevantNode))
            {
                yield return node;
            }
        }
    }

    private static bool IsRelevantNode(SyntaxNode node)
    {
        return node is IdentifierNameSyntax
            or MemberAccessExpressionSyntax
            or InvocationExpressionSyntax
            or ThisExpressionSyntax
            or BaseExpressionSyntax;
    }

    private static bool RequiresInstanceContext(ISymbol? symbol)
    {
        return symbol switch
        {
            IFieldSymbol field => !field.IsStatic,
            IPropertySymbol property => !property.IsStatic,
            IEventSymbol @event => !@event.IsStatic,
            IMethodSymbol method => !method.IsStatic && method.MethodKind switch
            {
                MethodKind.Ordinary or MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove => true,
                MethodKind.Constructor => true,
                MethodKind.LocalFunction => false,
                _ => false,
            },
            _ => false,
        };
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
