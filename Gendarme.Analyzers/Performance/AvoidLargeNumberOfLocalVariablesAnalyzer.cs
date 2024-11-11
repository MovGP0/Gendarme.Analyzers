namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidLargeNumberOfLocalVariablesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidLargeNumberOfLocalVariablesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidLargeNumberOfLocalVariablesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidLargeNumberOfLocalVariablesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidLargeNumberOfLocalVariables,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int DefaultMaximumVariables = 64;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    // Configuration property to allow customization of maximum variables
    private int MaximumVariables { get; set; } = DefaultMaximumVariables;

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method bodies
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        var semanticModel = context.SemanticModel;

        var localVariables = methodDeclaration.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Count();

        if (localVariables > MaximumVariables)
        {
            var methodName = methodDeclaration.Identifier.Text;
            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodName, localVariables, MaximumVariables);
            context.ReportDiagnostic(diagnostic);
        }
    }
}