namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConsiderCustomAccessorsForNonVisibleEventsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ConsiderCustomAccessorsForNonVisibleEventsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ConsiderCustomAccessorsForNonVisibleEventsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ConsiderCustomAccessorsForNonVisibleEventsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ConsiderCustomAccessorsForNonVisibleEvents,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze event symbols
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeEventSymbol, SymbolKind.Event);
    }

    private void AnalyzeEventSymbol(SymbolAnalysisContext context)
    {
        var eventSymbol = (IEventSymbol)context.Symbol;

        // Skip visible events
        if (eventSymbol.DeclaredAccessibility == Accessibility.Public || eventSymbol.DeclaredAccessibility == Accessibility.Protected)
            return;

        // Check if the add/remove methods are compiler-generated (synchronized)
        if (eventSymbol.AddMethod.IsImplicitlyDeclared && eventSymbol.RemoveMethod.IsImplicitlyDeclared)
        {
            var diagnostic = Diagnostic.Create(Rule, eventSymbol.Locations[0], eventSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}