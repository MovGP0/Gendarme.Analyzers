namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemoveUnusedLocalVariablesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.RemoveUnusedLocalVariables,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

        var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(methodDeclaration.Body);

        var declaredVariables = dataFlowAnalysis.VariablesDeclared;
        var usedVariables = dataFlowAnalysis.ReadInside.Union(dataFlowAnalysis.WrittenInside);

        foreach (var variable in declaredVariables)
        {
            if (!usedVariables.Contains(variable))
            {
                var location = variable.Locations.FirstOrDefault();
                if (location != null)
                {
                    var diagnostic = Diagnostic.Create(Rule, location, variable.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}