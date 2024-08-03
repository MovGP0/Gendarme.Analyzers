namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewCastOnIntegerDivisionAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ReviewCastOnIntegerDivision_Title;
    private static readonly LocalizableString MessageFormat = Strings.ReviewCastOnIntegerDivision_Message;
    private static readonly LocalizableString Description = Strings.ReviewCastOnIntegerDivision_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewCastOnIntegerDivision,
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
                SpecialType: SpecialType.System_Single or SpecialType.System_Double
            }
            || castExpression.Expression is not BinaryExpressionSyntax binaryExpr
            || !binaryExpr.IsKind(SyntaxKind.DivideExpression)
            || context.SemanticModel.GetTypeInfo(binaryExpr.Left).Type?.IsIntegralType() != true
            || context.SemanticModel.GetTypeInfo(binaryExpr.Right).Type?.IsIntegralType() != true)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, castExpression.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
