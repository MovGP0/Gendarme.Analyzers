namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemoveUnneededFinalizerAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.RemoveUnneededFinalizerTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.RemoveUnneededFinalizerMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.RemoveUnneededFinalizerDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.RemoveUnneededFinalizer,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        // Check if method is a finalizer (~ClassName)
        if (method.MethodKind != MethodKind.Destructor)
            return;

        var syntaxReference = method.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference == null)
            return;

        if (syntaxReference.GetSyntax(context.CancellationToken) is not DestructorDeclarationSyntax methodSyntax)
            return;

        // Check if finalizer is empty or only sets fields to null
        var statements = methodSyntax.Body?.Statements;
        if (statements == null || !statements.Any<StatementSyntax>())
        {
            // Finalizer is empty
            var diagnostic = Diagnostic.Create(Rule, methodSyntax.GetLocation(), method.ContainingType.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        bool onlyNullAssignments = true;
        foreach (var statement in statements)
        {
            if (statement is ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment }
                && assignment.Right.IsKind(SyntaxKind.NullLiteralExpression))
            {
                // Assignment to null
                continue;
            }
            else
            {
                onlyNullAssignments = false;
                break;
            }
        }

        if (onlyNullAssignments)
        {
            var diagnostic = Diagnostic.Create(Rule, methodSyntax.GetLocation(), method.ContainingType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}