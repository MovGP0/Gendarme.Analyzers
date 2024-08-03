namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotRoundIntegersAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotRoundIntegers_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotRoundIntegers_Message;
    private static readonly LocalizableString Description = Strings.DoNotRoundIntegers_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotRoundIntegers,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpression);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.Name is not ("Round" or "Ceiling" or "Floor" or "Truncate") ||
            methodSymbol.ContainingType.ToString() != "System.Math")
        {
            return;
        }

        var argumentType = context.SemanticModel.GetTypeInfo(invocationExpression.ArgumentList.Arguments.First().Expression).Type;
        if (argumentType != null && argumentType.IsIntegralType())
        {
            var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
