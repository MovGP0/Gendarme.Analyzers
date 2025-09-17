namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidRefAndOutParametersAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidRefAndOutParametersTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidRefAndOutParametersMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidRefAndOutParametersDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidRefAndOutParameters,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Skip property accessors / event accessors
        if (methodSymbol.MethodKind != MethodKind.Ordinary)
            return;

        // Externally visible only
        if (!methodSymbol.IsExternallyVisible())
            return;

        // Check if method is "TryXyz(out ...)" pattern
        var isTryPattern = methodSymbol.Name.StartsWith("Try") 
                           && methodSymbol.ReturnType.SpecialType == SpecialType.System_Boolean
                           && methodSymbol.Parameters.Any(p => p.RefKind == RefKind.Out);

        // For each parameter, see if it uses ref or out. If "Try" pattern, we allow out (but not ref).
        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.RefKind is RefKind.Ref or RefKind.Out)
            {
                // If it's the Try pattern with out, skip
                if (isTryPattern && parameter.RefKind == RefKind.Out)
                    continue;

                // Otherwise report
                var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0],
                    methodSymbol.Name, parameter.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}