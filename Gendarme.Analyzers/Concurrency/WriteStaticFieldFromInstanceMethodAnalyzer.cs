namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WriteStaticFieldFromInstanceMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.WriteStaticFieldFromInstanceMethod_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.WriteStaticFieldFromInstanceMethod_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.WriteStaticFieldFromInstanceMethod_Description), Strings.ResourceManager, typeof(Strings));

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