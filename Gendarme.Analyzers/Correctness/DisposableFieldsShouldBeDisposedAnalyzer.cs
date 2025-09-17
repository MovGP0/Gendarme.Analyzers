namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposableFieldsShouldBeDisposedAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DisposableFieldsShouldBeDisposed_Title;
    private static readonly LocalizableString MessageFormat = Strings.DisposableFieldsShouldBeDisposed_Message;
    private static readonly LocalizableString Description = Strings.DisposableFieldsShouldBeDisposed_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DisposableFieldsShouldBeDisposed,
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

        if (methodDeclaration.Identifier.Text != "Dispose"
            || methodDeclaration.Body is not { } body
            || context.ContainingSymbol is not { } containingSymbol)
        {
            return;
        }

        var disposableFields = containingSymbol.ContainingType
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => field.Type.AllInterfaces.Any(i => i.Name == "IDisposable"));

        foreach (var field in disposableFields)
        {
            if (!body.Statements
                .OfType<ExpressionStatementSyntax>()
                .Any(stmt => stmt.Expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax
                             {
                                 Expression: IdentifierNameSyntax identifier
                             } memberAccess } &&
                             identifier.Identifier.Text == field.Name &&
                             memberAccess.Name.Identifier.Text == "Dispose"))
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.GetLocation(), field.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
