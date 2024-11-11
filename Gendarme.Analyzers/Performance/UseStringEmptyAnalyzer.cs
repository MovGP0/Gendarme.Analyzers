namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseStringEmptyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseStringEmptyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseStringEmptyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseStringEmptyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseStringEmpty,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze literal expressions
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeLiteralExpression, SyntaxKind.StringLiteralExpression);
    }

    private void AnalyzeLiteralExpression(SyntaxNodeAnalysisContext context)
    {
        var literal = (LiteralExpressionSyntax)context.Node;

        if (literal.Token.ValueText == "")
        {
            var diagnostic = Diagnostic.Create(Rule, literal.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}