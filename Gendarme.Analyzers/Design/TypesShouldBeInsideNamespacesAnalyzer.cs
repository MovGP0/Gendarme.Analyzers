namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypesShouldBeInsideNamespacesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.TypesShouldBeInsideNamespacesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.TypesShouldBeInsideNamespacesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.TypesShouldBeInsideNamespacesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.TypesShouldBeInsideNamespaces,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
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
        // If not externally visible, ignore
        if (!namedType.DeclaredAccessibility.HasFlag(Accessibility.Public))
            return;

        // If the containing namespace is the global namespace or empty
        if (namedType.ContainingNamespace == null || namedType.ContainingNamespace.IsGlobalNamespace)
        {
            var diag = Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name);
            context.ReportDiagnostic(diag);
        }
    }
}