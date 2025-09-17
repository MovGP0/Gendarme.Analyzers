namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InternalNamespacesShouldNotExposeTypesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.InternalNamespacesShouldNotExposeTypesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.InternalNamespacesShouldNotExposeTypesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.InternalNamespacesShouldNotExposeTypesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.InternalNamespacesShouldNotExposeTypes,
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

        // If it's not publicly visible, ignore
        if (!namedType.DeclaredAccessibility.HasFlag(Accessibility.Public))
            return;

        // Check if the namespace ends with "Internal" or "Impl"
        string nsName = namedType.ContainingNamespace?.ToDisplayString() ?? "";
        if (nsName.EndsWith(".Internal") || nsName.EndsWith(".Impl"))
        {
            var diag = Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name,
                nsName);
            context.ReportDiagnostic(diag);
        }
    }
}