namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParameterNamesShouldMatchOverriddenMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ParameterNamesShouldMatchOverriddenMethodTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ParameterNamesShouldMatchOverriddenMethodMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ParameterNamesShouldMatchOverriddenMethodDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ParameterNamesShouldMatchOverriddenMethod,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        if (!methodSymbol.IsOverride && methodSymbol.ExplicitInterfaceImplementations.Length == 0)
            return;

        IMethodSymbol baseMethod = methodSymbol.OverriddenMethod ?? methodSymbol.ExplicitInterfaceImplementations.FirstOrDefault();
        if (baseMethod == null)
            return;

        var parameters = methodSymbol.Parameters;
        var baseParameters = baseMethod.Parameters;

        if (parameters.Length != baseParameters.Length)
            return;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].Name != baseParameters[i].Name)
            {
                var diagnostic = Diagnostic.Create(Rule, parameters[i].Locations[0], methodSymbol.Name, parameters[i].Name, baseParameters[i].Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}