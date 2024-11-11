namespace Gendarme.Analyzers.Security.Cas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewSuppressUnmanagedCodeSecurityUsageAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewSuppressUnmanagedCodeSecurityUsageTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewSuppressUnmanagedCodeSecurityUsageMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewSuppressUnmanagedCodeSecurityUsageDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewSuppressUnmanagedCodeSecurityUsage,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private const string SuppressUnmanagedCodeSecurityAttributeName = "System.Security.SuppressUnmanagedCodeSecurityAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types and methods
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method);
    }

    private void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;

        var hasSuppressAttribute = symbol.GetAttributes()
            .Any(attr => attr.AttributeClass.ToDisplayString() == SuppressUnmanagedCodeSecurityAttributeName);

        if (hasSuppressAttribute)
        {
            var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}