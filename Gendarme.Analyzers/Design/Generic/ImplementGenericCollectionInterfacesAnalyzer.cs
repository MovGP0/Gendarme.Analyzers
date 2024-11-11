namespace Gendarme.Analyzers.Design.Generic;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ImplementGenericCollectionInterfacesAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId.ImplementGenericCollectionInterfaces,
        title: "Implement Generic Collection Interfaces",
        messageFormat: "Consider implementing 'IEnumerable<T>' instead of 'IEnumerable' for type safety",
        category: "Design",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.Interfaces.Any(i => i is { Name: "IEnumerable", IsGenericType: false }))
        {
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}