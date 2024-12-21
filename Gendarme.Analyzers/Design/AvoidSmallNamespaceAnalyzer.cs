namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidSmallNamespaceAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidSmallNamespaceTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidSmallNamespaceMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidSmallNamespaceDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidSmallNamespace,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description
    );

    private const int DefaultMinimumTypes = 5;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // We'll gather info on all named types, then check after all symbols are processed.
        context.RegisterCompilationStartAction(compilationContext =>
        {
            var collector = new NamespaceTypeCountCollector();
            compilationContext.RegisterSymbolAction(collector.Collect, SymbolKind.NamedType);

            compilationContext.RegisterCompilationEndAction(collector.Report);
        });
    }

    private sealed class NamespaceTypeCountCollector
    {
        // Map namespace -> set of types
        private readonly Dictionary<string, HashSet<INamedTypeSymbol>> _namespaces = new();

        public void Collect(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            var ns = namedType.ContainingNamespace?.ToString() ?? string.Empty;

            // Skip special or internal namespaces (logic omitted; adapt as needed)
            if (!namedType.IsExternallyVisible())
                return;

            if (!_namespaces.TryGetValue(ns, out var set))
            {
                set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                _namespaces[ns] = set;
            }
            set.Add(namedType);
        }

        public void Report(CompilationAnalysisContext context)
        {
            // Evaluate namespace counts
            foreach (var kvp in _namespaces)
            {
                var namespaceName = kvp.Key;
                var typesInNamespace = kvp.Value;

                // Check if the count is less than the threshold
                if (typesInNamespace.Count < DefaultMinimumTypes)
                {
                    // For each type, we can report a diagnostic or just one for the namespace
                    foreach (var typeSymbol in typesInNamespace)
                    {
                        var diag = Diagnostic.Create(Rule, typeSymbol.Locations[0],
                            namespaceName, typesInNamespace.Count, DefaultMinimumTypes);
                        context.ReportDiagnostic(diag);
                    }
                }
            }
        }
    }
}