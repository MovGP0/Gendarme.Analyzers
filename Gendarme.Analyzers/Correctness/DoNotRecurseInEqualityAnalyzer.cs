namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotRecurseInEqualityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotRecurseInEquality_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotRecurseInEquality_Message;
    private static readonly LocalizableString Description = Strings.DoNotRecurseInEquality_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotRecurseInEquality,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.OperatorDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

        if (!operatorDeclaration.OperatorToken.IsKind(SyntaxKind.EqualsEqualsToken) &&
            !operatorDeclaration.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
        {
            return;
        }

        var body = operatorDeclaration.Body;

        if (body is null)
        {
            return;
        }

        if (!body.Statements.OfType<ExpressionStatementSyntax>().Any(stmt =>
                stmt.Expression is BinaryExpressionSyntax binaryExpr &&
                (binaryExpr.Left.IsEquivalentTo(operatorDeclaration.ParameterList.Parameters[0]) ||
                 binaryExpr.Right.IsEquivalentTo(operatorDeclaration.ParameterList.Parameters[1]))))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, operatorDeclaration.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
