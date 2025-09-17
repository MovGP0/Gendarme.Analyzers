namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotRecurseInEqualityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotRecurseInEquality_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotRecurseInEquality_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotRecurseInEquality_Description), Strings.ResourceManager, typeof(Strings));

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
