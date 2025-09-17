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
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeEventSymbol, SymbolKind.Event);
    }

    private static void AnalyzeEventSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IEventSymbol eventSymbol)
        {
            return;
        }

        if (eventSymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected)
        {
            return;
        }

        var addMethod = eventSymbol.AddMethod;
        var removeMethod = eventSymbol.RemoveMethod;
        if (addMethod is null || removeMethod is null)
        {
            return;
        }

        if (!addMethod.IsImplicitlyDeclared || !removeMethod.IsImplicitlyDeclared)
        {
            return;
        }

        var location = eventSymbol.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, location, eventSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}
