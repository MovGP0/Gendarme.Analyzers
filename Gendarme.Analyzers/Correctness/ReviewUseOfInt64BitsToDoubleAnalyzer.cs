namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewUseOfInt64BitsToDoubleAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ReviewUseOfInt64BitsToDouble_Title;
    private static readonly LocalizableString MessageFormat = Strings.ReviewUseOfInt64BitsToDouble_Message;
    private static readonly LocalizableString Description = Strings.ReviewUseOfInt64BitsToDouble_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewUseOfInt64BitsToDouble,
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

        var methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;
        if (methodSymbol?.Name != "Int64BitsToDouble" || methodSymbol.ContainingType?.Name != "BitConverter")
        {
            return;
        }

        var argumentList = invocationExpression.ArgumentList.Arguments;
        if (argumentList.Count == 1 && context.SemanticModel.GetTypeInfo(argumentList[0].Expression).Type?.SpecialType != SpecialType.System_Int64)
        {
            var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}