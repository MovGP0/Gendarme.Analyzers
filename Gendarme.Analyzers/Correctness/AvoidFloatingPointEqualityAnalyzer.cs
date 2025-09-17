namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidFloatingPointEqualityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.AvoidFloatingPointEquality_Title;
    private static readonly LocalizableString MessageFormat = Strings.AvoidFloatingPointEquality_Message;
    private static readonly LocalizableString Description = Strings.AvoidFloatingPointEquality_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidFloatingPointEquality,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.NotEqualsExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        var leftType = context.SemanticModel.GetTypeInfo(binaryExpression.Left).Type;
        var rightType = context.SemanticModel.GetTypeInfo(binaryExpression.Right).Type;

        if (leftType?.SpecialType is SpecialType.System_Double or SpecialType.System_Single &&
            rightType?.SpecialType is SpecialType.System_Double or SpecialType.System_Single)
        {
            var diagnostic = Diagnostic.Create(Rule, binaryExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
