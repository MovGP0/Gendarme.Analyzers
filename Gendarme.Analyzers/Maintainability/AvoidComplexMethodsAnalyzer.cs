namespace Gendarme.Analyzers.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidComplexMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidComplexMethodsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidComplexMethodsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidComplexMethodsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidComplexMethods,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int DefaultThreshold = 25; // This can be made configurable

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodSyntax, SyntaxKind.MethodDeclaration, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeMethodSyntax(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is MethodDeclarationSyntax methodDeclaration)
        {
            var complexity = CalculateCyclomaticComplexity(methodDeclaration);
            if (complexity > DefaultThreshold)
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodDeclaration.Identifier.ValueText, complexity);
                context.ReportDiagnostic(diagnostic);
            }
        }
        else if (context.Node is ConstructorDeclarationSyntax constructorDeclaration)
        {
            var complexity = CalculateCyclomaticComplexity(constructorDeclaration);
            if (complexity > DefaultThreshold)
            {
                var diagnostic = Diagnostic.Create(Rule, constructorDeclaration.Identifier.GetLocation(), constructorDeclaration.Identifier.ValueText, complexity);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static int CalculateCyclomaticComplexity(SyntaxNode node)
    {
        int complexity = 1;

        var descendants = node.DescendantNodes().ToArray();

        complexity += descendants.OfType<IfStatementSyntax>().Count();
        complexity += descendants.OfType<WhileStatementSyntax>().Count();
        complexity += descendants.OfType<ForStatementSyntax>().Count();
        complexity += descendants.OfType<ForEachStatementSyntax>().Count();
        complexity += descendants.OfType<CaseSwitchLabelSyntax>().Count();
        complexity += descendants.OfType<ConditionalExpressionSyntax>().Count();
        complexity += descendants.OfType<CatchClauseSyntax>().Count();
        complexity += descendants.OfType<BinaryExpressionSyntax>().Count(exp => exp.IsKind(SyntaxKind.LogicalAndExpression) || exp.IsKind(SyntaxKind.LogicalOrExpression));

        return complexity;
    }
}