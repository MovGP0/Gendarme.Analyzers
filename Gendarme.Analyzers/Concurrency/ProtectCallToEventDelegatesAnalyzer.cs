namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProtectCallToEventDelegatesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ProtectCallToEventDelegatesAnalyzer_Title;
    private static readonly LocalizableString MessageFormat = Strings.ProtectCallToEventDelegatesAnalyzer_Message;
    private static readonly LocalizableString Description = Strings.ProtectCallToEventDelegatesAnalyzer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ProtectCallToEventDelegates,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax expression)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(expression).Symbol is not IEventSymbol memberSymbol)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, expression.GetLocation(), memberSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}