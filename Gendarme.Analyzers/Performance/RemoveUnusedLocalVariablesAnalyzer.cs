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
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        if (context.SemanticModel is null || context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        var body = methodDeclaration.Body;
        if (body is null)
        {
            return;
        }

        var dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(body);
        if (dataFlowAnalysis is null)
        {
            return;
        }

        var usedVariables = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);
        AddSymbols(dataFlowAnalysis.ReadInside);
        AddSymbols(dataFlowAnalysis.WrittenInside);

        foreach (var variable in dataFlowAnalysis.VariablesDeclared)
        {
            if (usedVariables.Contains(variable))
            {
                continue;
            }

            var location = variable.Locations.FirstOrDefault();
            if (location is null || location == Location.None)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, location, variable.Name);
            context.ReportDiagnostic(diagnostic);
        }

        void AddSymbols(ImmutableArray<ISymbol> symbols)
        {
            if (symbols.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (var symbol in symbols)
            {
                usedVariables.Add(symbol);
            }
        }
    }
}
