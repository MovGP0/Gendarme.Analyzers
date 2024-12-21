namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferEventsOverMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.PreferEventsOverMethodsTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.PreferEventsOverMethodsMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.PreferEventsOverMethodsDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferEventsOverMethods,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

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
        if (methodSymbol.IsStatic) return;
        if (methodSymbol.MethodKind != MethodKind.Ordinary) return;

        // Very naive pattern: "RaiseXxx", "OnXxx", or "FireXxx" might be event-like
        var methodName = methodSymbol.Name;
        if (methodName.StartsWith("Raise") ||
            methodName.StartsWith("On") ||
            methodName.StartsWith("Fire"))
        {
            var diag = Diagnostic.Create(
                Rule,
                methodSymbol.Locations.FirstOrDefault(),
                methodSymbol.Name);
            context.ReportDiagnostic(diag);
        }
    }
}