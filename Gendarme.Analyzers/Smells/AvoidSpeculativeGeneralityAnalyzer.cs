namespace Gendarme.Analyzers.Smells;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidSpeculativeGeneralityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidSpeculativeGeneralityTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidSpeculativeGeneralityMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidSpeculativeGeneralityDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidSpeculativeGenerality,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Check for abstract classes with only one subclass
        if (namedTypeSymbol is { TypeKind: TypeKind.Class, IsAbstract: true })
        {
            context.Compilation.GetTypeByMetadataName(namedTypeSymbol.ToDisplayString())
                .AllInterfaces.SelectMany(i => i.AllInterfaces)
                .Distinct();

            var implementations = context.Compilation.GlobalNamespace.GetTypeMembers()
                .SelectMany(t => t.GetTypeMembers())
                .Where(t => t.BaseType?.Equals(namedTypeSymbol) == true);

            if (implementations.Count() == 1)
            {
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        // TODO: Further analysis for unnecessary delegation and unused parameters requires more complex analysis.
    }
}