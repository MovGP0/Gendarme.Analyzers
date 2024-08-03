namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FinalizersShouldCallBaseClassFinalizerAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.FinalizersShouldCallBaseClassFinalizer_Title;
    private static readonly LocalizableString MessageFormat = Strings.FinalizersShouldCallBaseClassFinalizer_Message;
    private static readonly LocalizableString Description = Strings.FinalizersShouldCallBaseClassFinalizer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.FinalizersShouldCallBaseClassFinalizer,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.DestructorDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DestructorDeclarationSyntax
            {
                Parent: {} parent
            } destructorDeclaration
            || context.SemanticModel.GetDeclaredSymbol(parent) is not INamedTypeSymbol
            {
                BaseType: {} baseType
            }
            || baseType.SpecialType == SpecialType.System_Object
            || destructorDeclaration.Body is not {} body
            || body.Statements
                .OfType<ExpressionStatementSyntax>()
                .Any(stmt => stmt.Expression is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Expression: BaseExpressionSyntax,
                        Name.Identifier.Text: "Finalize"
                    }
                }))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, destructorDeclaration.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
