namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewUseOfModuloOneOnIntegersAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ReviewUseOfModuloOneOnIntegers_Title;
    private static readonly LocalizableString MessageFormat = Strings.ReviewUseOfModuloOneOnIntegers_Message;
    private static readonly LocalizableString Description = Strings.ReviewUseOfModuloOneOnIntegers_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewUseOfModuloOneOnIntegers,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ModuloExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        if (binaryExpression.Right is not LiteralExpressionSyntax literal
            || literal.Token.ValueText != "1")
        {
            return;
        }

        var leftType = context.SemanticModel.GetTypeInfo(binaryExpression.Left).Type;
        if (leftType?.IsIntegralType() != true)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, binaryExpression.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}