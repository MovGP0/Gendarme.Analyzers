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
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.TypeKind != TypeKind.Class || !namedTypeSymbol.IsAbstract)
        {
            return;
        }

        var implementationsCount = context.Compilation.GlobalNamespace.GetTypeMembers()
            .SelectMany(type => type.GetTypeMembers())
            .Count(type => SymbolEqualityComparer.Default.Equals(type.BaseType, namedTypeSymbol));

        if (implementationsCount == 1 && namedTypeSymbol.Locations.Length > 0)
        {
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
