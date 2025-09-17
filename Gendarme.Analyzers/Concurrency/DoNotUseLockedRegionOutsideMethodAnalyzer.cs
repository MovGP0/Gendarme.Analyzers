namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseLockedRegionOutsideMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotUseLockedRegionOutsideMethodAnalyzer_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotUseLockedRegionOutsideMethodAnalyzer_Message;
    private static readonly LocalizableString Description = Strings.DoNotUseLockedRegionOutsideMethodAnalyzer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotUseLockedRegionOutsideMethod,
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
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            return;

        if (methodSymbol.ContainingType.ToString() == "System.Threading.Monitor" && methodSymbol.Name == "Enter")
        {
            var methodDeclaration = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDeclaration == null)
                return;

            var hasExit = methodDeclaration.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(e => context.SemanticModel.GetSymbolInfo(e).Symbol is IMethodSymbol m && m.ContainingType.ToString() == "System.Threading.Monitor" && m.Name == "Exit");

            if (!hasExit)
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}