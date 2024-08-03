namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewUselessControlFlowAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ReviewUselessControlFlow_Title;
    private static readonly LocalizableString MessageFormat = Strings.ReviewUselessControlFlow_Message;
    private static readonly LocalizableString Description = Strings.ReviewUselessControlFlow_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewUselessControlFlow,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Block);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var block = (BlockSyntax)context.Node;

        if (block.Statements.Any())
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, block.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}