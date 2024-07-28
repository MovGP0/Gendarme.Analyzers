namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConstructorShouldNotCallVirtualMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ConstructorShouldNotCallVirtualMethods_Title;
    private static readonly LocalizableString MessageFormat = Strings.ConstructorShouldNotCallVirtualMethods_Message;
    private static readonly LocalizableString Description = Strings.ConstructorShouldNotCallVirtualMethods_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ConstructorShouldNotCallVirtualMethods,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
        var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;

        var containingType = context.SemanticModel.GetDeclaredSymbol(constructorDeclaration)?.ContainingType;
        if (containingType == null || containingType.IsSealed)
        {
            return;
        }

        foreach (var descendantNode in constructorDeclaration.DescendantNodes())
        {
            if (descendantNode is not InvocationExpressionSyntax invocationExpression)
            {
                continue;
            }

            if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is IMethodSymbol { IsVirtual: true } methodSymbol
                && SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, containingType))
            {
                var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(), methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
