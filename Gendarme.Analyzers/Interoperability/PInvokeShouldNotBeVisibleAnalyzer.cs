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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        var dllImportAttribute = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.DllImportAttribute");
        if (dllImportAttribute is null)
        {
            return;
        }

        var hasDllImport = methodSymbol.GetAttributes()
            .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, dllImportAttribute));
        if (!hasDllImport)
        {
            return;
        }

        if (methodSymbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal)
        {
            var location = methodSymbol.Locations.FirstOrDefault();
            if (location is not null)
            {
                var diagnostic = Diagnostic.Create(Rule, location, methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
