namespace Gendarme.Analyzers.Smells;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidSwitchStatementsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidSwitchStatementsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidSwitchStatementsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidSwitchStatementsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidSwitchStatements,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze switch statements
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
    }

    private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
    {
        var switchStatement = (SwitchStatementSyntax)context.Node;

        var methodDeclaration = switchStatement.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration != null)
        {
            var diagnostic = Diagnostic.Create(Rule, switchStatement.SwitchKeyword.GetLocation(), methodDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }
}