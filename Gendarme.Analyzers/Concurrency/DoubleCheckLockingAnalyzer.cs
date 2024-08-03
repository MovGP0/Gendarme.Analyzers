namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoubleCheckLockingAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoubleCheckLockingAnalyzer_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoubleCheckLockingAnalyzer_Message;
    private static readonly LocalizableString Description = Strings.DoubleCheckLockingAnalyzer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoubleCheckLocking,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IfStatement);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;
        var lockStatement = ifStatement.Statement as BlockSyntax;

        if (lockStatement == null)
            return;

        var lockStatements = lockStatement.Statements.OfType<LockStatementSyntax>().ToList();

        if (lockStatements.Count != 1)
            return;

        var innerIfStatements = lockStatements[0].Statement.DescendantNodes().OfType<IfStatementSyntax>().ToList();

        if (innerIfStatements.Count != 1)
            return;

        var diagnostic = Diagnostic.Create(Rule, ifStatement.GetLocation(), lockStatements[0].Expression.ToString());
        context.ReportDiagnostic(diagnostic);
    }
}