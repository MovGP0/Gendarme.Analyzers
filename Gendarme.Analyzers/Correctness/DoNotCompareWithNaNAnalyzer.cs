namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotCompareWithNaNAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotCompareWithNaN_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotCompareWithNaN_Message;
    private static readonly LocalizableString Description = Strings.DoNotCompareWithNaN_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotCompareWithNaN,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.EqualsExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        if (!IsNaNComparison(context, binaryExpression.Left) &&
            !IsNaNComparison(context, binaryExpression.Right))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, binaryExpression.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsNaNComparison(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        var symbolInfo = context.SemanticModel.GetSymbolInfo(expression);
        return symbolInfo.Symbol is { Name: "NaN" } && 
               symbolInfo.Symbol.ContainingType.Name is "Double" or "Single";
    }
}