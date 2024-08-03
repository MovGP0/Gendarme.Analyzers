namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotLockOnThisOrTypesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotLockOnThisOrTypesAnalyzer_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotLockOnThisOrTypesAnalyzer_Message;
    private static readonly LocalizableString Description = Strings.DoNotLockOnThisOrTypesAnalyzer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotLockOnThisOrTypes,
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
        var expression = lockStatement.Expression;

        if (expression.IsKind(SyntaxKind.ThisExpression) || expression.IsKind(SyntaxKind.TypeOfExpression))
        {
            var diagnostic = Diagnostic.Create(Rule, expression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}