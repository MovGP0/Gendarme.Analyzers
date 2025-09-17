namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotForgetNotImplementedMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotForgetNotImplementedMethods_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotForgetNotImplementedMethods_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotForgetNotImplementedMethods_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotForgetNotImplementedMethods,
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
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        // Only analyze short methods (e.g., 1-2 lines)
        if (methodDeclaration.Body == null || methodDeclaration.Body.Statements.Count > 2)
        {
            return;
        }

        foreach (var statement in methodDeclaration.Body.Statements)
        {
            if (statement is ThrowStatementSyntax { Expression: ObjectCreationExpressionSyntax throwExpression } throwStatement
                && context.SemanticModel.GetSymbolInfo(throwExpression.Type).Symbol is INamedTypeSymbol typeSymbol
                && typeSymbol.ToString() == "System.NotImplementedException")
            {
                var diagnostic = Diagnostic.Create(Rule, throwStatement.GetLocation(), methodDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}