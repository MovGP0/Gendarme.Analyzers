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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Method);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var suppressAttributeType = context.Compilation.GetTypeByMetadataName(SuppressUnmanagedCodeSecurityAttributeName);
        if (suppressAttributeType is null)
        {
            return;
        }

        var symbol = context.Symbol;
        var hasSuppressAttribute = symbol.GetAttributes()
            .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, suppressAttributeType));

        if (!hasSuppressAttribute)
        {
            return;
        }

        var location = symbol.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, location, symbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}
