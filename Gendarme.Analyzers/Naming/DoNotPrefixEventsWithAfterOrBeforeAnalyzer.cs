namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotPrefixEventsWithAfterOrBeforeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotPrefixEventsWithAfterOrBeforeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotPrefixEventsWithAfterOrBeforeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotPrefixEventsWithAfterOrBeforeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotPrefixEventsWithAfterOrBefore,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<string> DisallowedPrefixes = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "After", "Before");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeEvent, SymbolKind.Event);
    }

    private static void AnalyzeEvent(SymbolAnalysisContext context)
    {
        var eventSymbol = (IEventSymbol)context.Symbol;
        var eventName = eventSymbol.Name;

        foreach (var prefix in DisallowedPrefixes)
        {
            if (eventName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var diagnostic = Diagnostic.Create(Rule, eventSymbol.Locations[0], eventName);
                context.ReportDiagnostic(diagnostic);
                break;
            }
        }
    }
}