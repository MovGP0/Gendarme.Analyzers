namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProtectCallToEventDelegatesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ProtectCallToEventDelegatesAnalyzer_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ProtectCallToEventDelegatesAnalyzer_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ProtectCallToEventDelegatesAnalyzer_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ProtectCallToEventDelegates,
        Title,
        MessageFormat,
        Category.Concurrency,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Parent is ConditionalAccessExpressionSyntax)
        {
            return;
        }

        var eventSymbol = GetEventSymbol(invocation, context.SemanticModel, context.CancellationToken);
        if (eventSymbol is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Expression.GetLocation(), eventSymbol.Name));
    }

    private static IEventSymbol? GetEventSymbol(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier => TryGetEventSymbol(identifier, semanticModel, cancellationToken),
            MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Invoke" } memberAccess => TryGetEventSymbol(memberAccess.Expression, semanticModel, cancellationToken),
            _ => null,
        };
    }

    private static IEventSymbol? TryGetEventSymbol(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(expression, cancellationToken);
        var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

        switch (symbol)
        {
            case IEventSymbol eventSymbol:
                return eventSymbol;
            case IFieldSymbol { AssociatedSymbol: IEventSymbol associatedEvent }:
                return associatedEvent;
            case IMethodSymbol { MethodKind: MethodKind.DelegateInvoke } methodSymbol:
                return FindEventSymbol(expression, semanticModel, cancellationToken, methodSymbol.ContainingType);
        }

        return null;
    }

    private static IEventSymbol? FindEventSymbol(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, ITypeSymbol delegateType)
    {
        var name = expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            _ => null,
        };

        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        foreach (var candidate in semanticModel.LookupSymbols(expression.SpanStart, name: name))
        {
            if (candidate is IEventSymbol eventSymbol && SymbolEqualityComparer.Default.Equals(eventSymbol.Type, delegateType))
            {
                return eventSymbol;
            }
        }

        return null;
    }
}
