namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotSwallowErrorsCatchingNonSpecificExceptionsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotSwallowErrorsCatchingNonSpecificExceptionsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotSwallowErrorsCatchingNonSpecificExceptionsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotSwallowErrorsCatchingNonSpecificExceptionsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotSwallowErrorsCatchingNonSpecificExceptions,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
    }

    private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
    {
        var catchClause = (CatchClauseSyntax)context.Node;

        var exceptionType = catchClause.Declaration?.Type;
        if (exceptionType == null)
            return;

        var typeSymbol = context.SemanticModel.GetTypeInfo(exceptionType).Type as INamedTypeSymbol;
        if (typeSymbol == null)
            return;

        if (typeSymbol.ToString() == "System.Exception" || typeSymbol.ToString() == "System.SystemException")
        {
            var hasThrowStatement = catchClause.Block.DescendantNodes().OfType<ThrowStatementSyntax>().Any();
            if (!hasThrowStatement)
            {
                var diagnostic = Diagnostic.Create(Rule, catchClause.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}