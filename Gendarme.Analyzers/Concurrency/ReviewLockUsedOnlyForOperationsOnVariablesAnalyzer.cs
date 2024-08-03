namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer_Title;
    private static readonly LocalizableString MessageFormat = Strings.ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer_Message;
    private static readonly LocalizableString Description = Strings.ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewLockUsedOnlyForOperationsOnVariables,
        Title,
        MessageFormat,
        Category.Concurrency,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LockStatement);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var lockStatement = (LockStatementSyntax)context.Node;
        var lockExpression = lockStatement.Expression;

        if (lockStatement.Statement is not BlockSyntax lockBody)
        {
            return;
        }

        foreach (var statement in lockBody.Statements)
        {
            if (statement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment })
            {
                continue;
            }

            if (context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol is not IFieldSymbol { IsStatic: true } leftSymbol)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, lockExpression.GetLocation(), lockExpression.ToString());
            context.ReportDiagnostic(diagnostic);
        }
    }
}