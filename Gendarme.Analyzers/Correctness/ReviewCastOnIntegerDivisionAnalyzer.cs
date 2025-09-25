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
        context.RegisterSyntaxNodeAction(AnalyzeCast, SyntaxKind.CastExpression);
    }

    private static void AnalyzeCast(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CastExpressionSyntax castExpression)
        {
            return;
        }

        var targetType = context.SemanticModel.GetTypeInfo(castExpression.Type, context.CancellationToken).Type;
        if (targetType is null || !IsFloatingPointOrDecimal(targetType))
        {
            return;
        }

        var expression = RemoveParentheses(castExpression.Expression);
        if (expression is not BinaryExpressionSyntax binaryExpression || !binaryExpression.IsKind(SyntaxKind.DivideExpression))
        {
            return;
        }

        var leftType = context.SemanticModel.GetTypeInfo(binaryExpression.Left, context.CancellationToken).Type;
        var rightType = context.SemanticModel.GetTypeInfo(binaryExpression.Right, context.CancellationToken).Type;
        if (leftType?.IsIntegralType() != true || rightType?.IsIntegralType() != true)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, castExpression.GetLocation()));
    }

    private static bool IsFloatingPointOrDecimal(ITypeSymbol type)
    {
        return type.SpecialType is SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal;
    }

    private static ExpressionSyntax RemoveParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }
}
