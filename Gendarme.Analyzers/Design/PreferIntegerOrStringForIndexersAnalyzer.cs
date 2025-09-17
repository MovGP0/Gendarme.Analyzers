namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferIntegerOrStringForIndexersAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.PreferIntegerOrStringForIndexersTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.PreferIntegerOrStringForIndexersMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.PreferIntegerOrStringForIndexersDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferIntegerOrStringForIndexers,
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
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        // Find indexers (properties named "this[]")
        var indexers = namedType.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.IsIndexer);

        foreach (var indexer in indexers)
        {
            // If the indexer uses anything other than int, long, or string, warn
            foreach (var param in indexer.Parameters)
            {
                var paramType = param.Type;
                if (paramType.SpecialType != SpecialType.System_Int32 &&
                    paramType.SpecialType != SpecialType.System_Int64 &&
                    paramType.SpecialType != SpecialType.System_String)
                {
                    var diag = Diagnostic.Create(
                        Rule,
                        indexer.Locations.FirstOrDefault(),
                        namedType.Name,
                        paramType.Name);
                    context.ReportDiagnostic(diag);
                }
            }
        }
    }
}