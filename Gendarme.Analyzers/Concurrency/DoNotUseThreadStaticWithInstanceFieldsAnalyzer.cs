namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseThreadStaticWithInstanceFieldsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotUseThreadStaticWithInstanceFieldsAnalyzer_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotUseThreadStaticWithInstanceFieldsAnalyzer_Message;
    private static readonly LocalizableString Description = Strings.DoNotUseThreadStaticWithInstanceFieldsAnalyzer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotUseThreadStaticWithInstanceFields,
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

        if (!fieldSymbol.IsStatic && fieldSymbol.GetAttributes().Any(attr => attr.AttributeClass?.ToString() == "System.ThreadStaticAttribute"))
        {
            var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}