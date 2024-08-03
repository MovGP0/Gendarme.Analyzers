namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WriteStaticFieldFromInstanceMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.WriteStaticFieldFromInstanceMethod_Title;
    private static readonly LocalizableString MessageFormat = Strings.WriteStaticFieldFromInstanceMethod_Message;
    private static readonly LocalizableString Description = Strings.WriteStaticFieldFromInstanceMethod_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.WriteStaticFieldFromInstanceMethod,
        Title,
        MessageFormat,
        Category.Concurrency,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleAssignmentExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol is not IFieldSymbol { IsStatic: true } leftSymbol)
        {
            return;
        }

        var containingMethod = assignment.FirstAncestorOrSelf<MethodDeclarationSyntax>();

        if (containingMethod == null || containingMethod.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, assignment.GetLocation(), containingMethod.Identifier.Text, leftSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}