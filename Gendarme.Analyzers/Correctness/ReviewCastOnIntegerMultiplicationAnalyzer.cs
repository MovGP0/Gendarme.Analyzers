namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewCastOnIntegerMultiplicationAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewCastOnIntegerMultiplication_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewCastOnIntegerMultiplication_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewCastOnIntegerMultiplication_Description), Strings.ResourceManager, typeof(Strings));

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
        if (context.Node is not CastExpressionSyntax castExpression)
        {
            return;
        }

        if (context.SemanticModel.GetTypeInfo(castExpression.Type).Type is not
            {
                SpecialType: SpecialType.System_Int64
            })
        {
            return;
        }

        var expression = Unwrap(castExpression.Expression);

        if (expression is not BinaryExpressionSyntax binaryExpression || !binaryExpression.IsKind(SyntaxKind.MultiplyExpression))
        {
            return;
        }

        var leftInfo = context.SemanticModel.GetTypeInfo(binaryExpression.Left);
        var rightInfo = context.SemanticModel.GetTypeInfo(binaryExpression.Right);
        var leftType = leftInfo.Type ?? leftInfo.ConvertedType;
        var rightType = rightInfo.Type ?? rightInfo.ConvertedType;

        if (leftType?.SpecialType != SpecialType.System_Int32 || rightType?.SpecialType != SpecialType.System_Int32)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, castExpression.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }
}
