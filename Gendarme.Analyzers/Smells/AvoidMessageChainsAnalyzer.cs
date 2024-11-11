namespace Gendarme.Analyzers.Smells;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidMessageChainsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidMessageChainsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidMessageChainsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidMessageChainsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidMessageChains,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int MaxChainLength = 3;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze member access expressions
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        int chainLength = 1;
        var expression = memberAccess.Expression;

        while (expression is MemberAccessExpressionSyntax innerMemberAccess)
        {
            chainLength++;
            expression = innerMemberAccess.Expression;

            if (chainLength > MaxChainLength)
            {
                var methodDeclaration = memberAccess.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                if (methodDeclaration != null)
                {
                    var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation(), methodDeclaration.Identifier.Text);
                    context.ReportDiagnostic(diagnostic);
                }
                break;
            }
        }
    }
}