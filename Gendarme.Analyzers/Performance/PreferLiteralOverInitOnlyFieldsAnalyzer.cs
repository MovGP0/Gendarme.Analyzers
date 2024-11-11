namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferLiteralOverInitOnlyFieldsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferLiteralOverInitOnlyFields,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze field symbols
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
    }

    private void AnalyzeFieldSymbol(SymbolAnalysisContext context)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;

        // Check if field is static readonly
        if (!fieldSymbol.IsStatic || !fieldSymbol.IsReadOnly)
            return;

        // Check if field is initialized with a compile-time constant
        if (fieldSymbol.HasConstantValue)
        {
            var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}