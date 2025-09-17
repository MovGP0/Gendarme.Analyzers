namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReplaceIncompleteOddnessCheckAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReplaceIncompleteOddnessCheck_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReplaceIncompleteOddnessCheck_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReplaceIncompleteOddnessCheck_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReplaceIncompleteOddnessCheck,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        if (binaryExpression.Left is ParenthesizedExpressionSyntax { Expression: BinaryExpressionSyntax moduloExpr } &&
            moduloExpr.OperatorToken.IsKind(SyntaxKind.PercentToken) &&
            moduloExpr.Right is LiteralExpressionSyntax { Token.ValueText: "2" } &&
            binaryExpression.Right is LiteralExpressionSyntax { Token.ValueText: "1" })
        {
            var diagnostic = Diagnostic.Create(Rule, binaryExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}