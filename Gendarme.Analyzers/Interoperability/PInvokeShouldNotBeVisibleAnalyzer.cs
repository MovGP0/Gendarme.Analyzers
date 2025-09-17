namespace Gendarme.Analyzers.Interoperability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PInvokeShouldNotBeVisibleAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.PInvokeShouldNotBeVisibleTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.PInvokeShouldNotBeVisibleMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.PInvokeShouldNotBeVisibleDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PInvokeShouldNotBeVisible,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Check for the presence of the DllImportAttribute to identify P/Invoke methods
        var hasDllImport = methodSymbol.GetAttributes().Any(attr => attr.AttributeClass.ToString() == "System.Runtime.InteropServices.DllImportAttribute");
        if (!hasDllImport)
            return;

        // Check if the method's visibility is public or protected
        if (methodSymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal)
        {
            var location = methodSymbol.Locations.FirstOrDefault();
            if (location != null)
            {
                var diagnostic = Diagnostic.Create(Rule, location, methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}