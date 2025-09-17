namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoubleCheckLockingAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoubleCheckLockingAnalyzer_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoubleCheckLockingAnalyzer_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoubleCheckLockingAnalyzer_Description), Strings.ResourceManager, typeof(Strings));

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

        if (ifStatement.Statement is not BlockSyntax lockStatement)
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