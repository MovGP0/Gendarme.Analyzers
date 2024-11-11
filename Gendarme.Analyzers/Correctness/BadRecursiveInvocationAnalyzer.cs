namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BadRecursiveInvocationAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.BadRecursiveInvocation_Title;
    private static readonly LocalizableString MessageFormat = Strings.BadRecursiveInvocation_Message;
    private static readonly LocalizableString Description = Strings.BadRecursiveInvocation_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.BadRecursiveInvocation,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        switch (context.Node)
        {
            case MethodDeclarationSyntax methodDeclaration:
                AnalyzeMethod(context, methodDeclaration);
                break;
            case PropertyDeclarationSyntax propertyDeclaration:
                AnalyzeProperty(context, propertyDeclaration);
                break;
        }
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration)
    {
        var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
        if (methodSymbol == null)
        {
            return;
        }

        var invocations = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
        foreach (var invocation in invocations)
        {
            if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedSymbol
                && SymbolEqualityComparer.Default.Equals(invokedSymbol, methodSymbol))
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context, PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.AccessorList is not { } accessorList)
        {
            return;
        }

        foreach (var accessor in accessorList.Accessors)
        {
            var accessorSymbol = context.SemanticModel.GetDeclaredSymbol(accessor);
            if (accessorSymbol == null)
            {
                return;
            }

            var invocations = accessor.Body?.DescendantNodes().OfType<InvocationExpressionSyntax>() ?? [];
            foreach (var invocation in invocations)
            {
                if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol invokedSymbol
                    && SymbolEqualityComparer.Default.Equals(invokedSymbol,accessorSymbol))
                {
                    var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), propertyDeclaration.Identifier.Text);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
