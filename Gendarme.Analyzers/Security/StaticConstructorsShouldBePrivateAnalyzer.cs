namespace Gendarme.Analyzers.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StaticConstructorsShouldBePrivateAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.StaticConstructorsShouldBePrivateTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.StaticConstructorsShouldBePrivateMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.StaticConstructorsShouldBePrivateDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.StaticConstructorsShouldBePrivate,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze constructor declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Check if the method is a static constructor
        if (methodSymbol.MethodKind != MethodKind.StaticConstructor)
            return;

        // Check if the accessibility is not private
        if (methodSymbol.DeclaredAccessibility != Accessibility.Private)
        {
            var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.ContainingType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}