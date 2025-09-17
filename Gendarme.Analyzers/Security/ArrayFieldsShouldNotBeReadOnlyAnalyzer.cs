namespace Gendarme.Analyzers.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ArrayFieldsShouldNotBeReadOnlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ArrayFieldsShouldNotBeReadOnlyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ArrayFieldsShouldNotBeReadOnlyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ArrayFieldsShouldNotBeReadOnlyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ArrayFieldsShouldNotBeReadOnly,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze field declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
    }

    private void AnalyzeFieldSymbol(SymbolAnalysisContext context)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;

        // Check if the field is an array
        if (fieldSymbol.Type.Kind != SymbolKind.ArrayType)
            return;

        // Check if the field is public and readonly
        if (fieldSymbol is { DeclaredAccessibility: Accessibility.Public, IsReadOnly: true })
        {
            var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}