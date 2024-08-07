namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisableDebuggingCodeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DisableDebuggingCode_Title;
    private static readonly LocalizableString MessageFormat = Strings.DisableDebuggingCode_Message;
    private static readonly LocalizableString Description = Strings.DisableDebuggingCode_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DisableDebuggingCode,
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
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(memberAccess).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (methodSymbol.ContainingType.ToString() != "System.Console" || methodSymbol.Name != "WriteLine")
        {
            return;
        }

        var typeDeclaration = invocation.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
        if (typeDeclaration == null)
        {
            return;
        }

        var containingType = context.SemanticModel.GetDeclaredSymbol(typeDeclaration);
        if (containingType == null)
        {
            return;
        }

        if (containingType.AllInterfaces.All(i => i.ToString() != "System.IConsoleApplication"))
        {
            var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation(), containingType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
