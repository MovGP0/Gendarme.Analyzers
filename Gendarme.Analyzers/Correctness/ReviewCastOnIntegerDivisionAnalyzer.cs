namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewCastOnIntegerDivisionAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewCastOnIntegerDivision_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewCastOnIntegerDivision_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewCastOnIntegerDivision_Description), Strings.ResourceManager, typeof(Strings));

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
