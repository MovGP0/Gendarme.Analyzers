namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnsureLocalDisposalAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.EnsureLocalDisposal_Title;
    private static readonly LocalizableString MessageFormat = Strings.EnsureLocalDisposal_Message;
    private static readonly LocalizableString Description = Strings.EnsureLocalDisposal_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.EnsureLocalDisposal,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var disposableLocals = methodDeclaration.Body
            ?.DescendantNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .SelectMany(localDecl => localDecl.Declaration.Variables)
            .Where(variable => 
                variable.Initializer?.Value is {} value &&
                context.SemanticModel.GetTypeInfo(value).Type?.AllInterfaces
            .Any(i => i.Name == "IDisposable") == true)
            .ToList();

        if (disposableLocals == null || !disposableLocals.Any())
        {
            return;
        }

        foreach (var local in disposableLocals)
        {
            var body = methodDeclaration.Body;
            if (body is null)
            {
                continue;
            }

            var hasDisposeCall = body
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(invocation => IsDisposeCallForLocal(invocation, local));

            if (hasDisposeCall)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, local.GetLocation(), local.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsDisposeCallForLocal(
        InvocationExpressionSyntax invocation,
        VariableDeclaratorSyntax local)
    {
        return invocation.Expression is MemberAccessExpressionSyntax
        {
            Name.Identifier.Text: "Dispose",
            Expression: IdentifierNameSyntax identifier
        } && identifier.Identifier.Text == local.Identifier.Text;
    }
}
