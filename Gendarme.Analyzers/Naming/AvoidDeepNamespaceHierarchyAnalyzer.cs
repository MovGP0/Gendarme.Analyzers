namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidDeepNamespaceHierarchyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidDeepNamespaceHierarchyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidDeepNamespaceHierarchyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidDeepNamespaceHierarchyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidDeepNamespaceHierarchy,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    // Configurable maximum depth
    private const int DefaultMaxDepth = 4;
    private static readonly ImmutableHashSet<string> AllowedSpecializations = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "Design", "Interop", "Permissions", "Internal");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamespace, SymbolKind.Namespace);
    }

    private static void AnalyzeNamespace(SymbolAnalysisContext context)
    {
        var namespaceSymbol = (INamespaceSymbol)context.Symbol;
        var namespaceName = namespaceSymbol.ToDisplayString();

        var parts = namespaceName.Split('.');
        var depth = parts.Length;

        if (depth > DefaultMaxDepth)
        {
            var nextPart = parts[DefaultMaxDepth];
            if (!AllowedSpecializations.Contains(nextPart) && !nextPart.StartsWith("_"))
            {
                var diagnostic = Diagnostic.Create(Rule, namespaceSymbol.Locations[0], namespaceName, depth);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}