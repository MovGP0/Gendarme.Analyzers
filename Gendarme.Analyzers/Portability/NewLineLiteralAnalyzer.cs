namespace Gendarme.Analyzers.Portability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NewLineLiteralAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.NewLineLiteralTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.NewLineLiteralMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.NewLineLiteralDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.NewLineLiteral,
        Title,
        MessageFormat,
        Category.Portability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze string literals
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterSyntaxNodeAction(AnalyzeLiteralExpression, SyntaxKind.StringLiteralExpression);
    }

    private static void AnalyzeLiteralExpression(SyntaxNodeAnalysisContext context)
    {
        var literalExpression = (LiteralExpressionSyntax)context.Node;
        var literalValue = literalExpression.Token.ValueText;

        if (literalValue.Contains("\n") || literalValue.Contains("\r"))
        {
            var diagnostic = Diagnostic.Create(Rule, literalExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}