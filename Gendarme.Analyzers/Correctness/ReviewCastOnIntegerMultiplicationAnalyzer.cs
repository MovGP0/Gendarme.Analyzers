namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewCastOnIntegerMultiplicationAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ReviewCastOnIntegerMultiplication_Title;
    private static readonly LocalizableString MessageFormat = Strings.ReviewCastOnIntegerMultiplication_Message;
    private static readonly LocalizableString Description = Strings.ReviewCastOnIntegerMultiplication_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewCastOnIntegerMultiplication,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.CastExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CastExpressionSyntax castExpression
            || context.SemanticModel.GetTypeInfo(castExpression.Type).Type is not
            {
                SpecialType: SpecialType.System_Int64
            }
            || castExpression.Expression is not BinaryExpressionSyntax binaryExpr
            || !binaryExpr.IsKind(SyntaxKind.MultiplyExpression)
            || context.SemanticModel.GetTypeInfo(binaryExpr.Left).Type?.SpecialType != SpecialType.System_Int32
            || context.SemanticModel.GetTypeInfo(binaryExpr.Right).Type?.SpecialType != SpecialType.System_Int32)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, castExpression.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
