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
        private readonly object _gate = new();

        public void Collect(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            var ns = namedType.ContainingNamespace?.ToString() ?? string.Empty;

            // Skip special or internal namespaces (logic omitted; adapt as needed)
            if (!namedType.IsExternallyVisible())
                return;

            lock (_gate)
            {
                if (!_namespaces.TryGetValue(ns, out var set))
                {
                    set = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
                    _namespaces[ns] = set;
                }
                set.Add(namedType);
            }
        }

        public void Report(CompilationAnalysisContext context)
        {
            Dictionary<string, HashSet<INamedTypeSymbol>> snapshot;
            lock (_gate)
            {
                snapshot = _namespaces.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value,
                    StringComparer.Ordinal);
            }

            foreach (var kvp in snapshot)
            {
                var namespaceName = kvp.Key;
                var typesInNamespace = kvp.Value;

                // Check if the count is less than the threshold
                if (typesInNamespace.Count < DefaultMinimumTypes)
                {
                    // Report a single diagnostic per namespace, attached to the first type declaration location
                    var firstType = typesInNamespace
                        .OrderBy(t => t.Locations.FirstOrDefault()?.GetLineSpan().StartLinePosition.Line ?? int.MaxValue)
                        .FirstOrDefault();

                    var location = firstType?.Locations.FirstOrDefault();
                    if (location is not null)
                    {
                        var diag = Diagnostic.Create(Rule, location,
                            namespaceName, typesInNamespace.Count, DefaultMinimumTypes);
                        context.ReportDiagnostic(diag);
                    }
                }
            }
        }
    }
}