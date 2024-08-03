namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonConstantStaticFieldsShouldNotBeVisibleAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.NonConstantStaticFieldsShouldNotBeVisible_Title;
    private static readonly LocalizableString MessageFormat = Strings.NonConstantStaticFieldsShouldNotBeVisible_Message;
    private static readonly LocalizableString Description = Strings.NonConstantStaticFieldsShouldNotBeVisible_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.NonConstantStaticFieldsShouldNotBeVisible,
        Title,
        MessageFormat,
        Category.Concurrency,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Field);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;

        if (fieldSymbol is { IsStatic: true, DeclaredAccessibility: Accessibility.Public, IsConst: false, IsReadOnly: false })
        {
            var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}