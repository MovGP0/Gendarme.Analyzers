namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidReturningArraysOnPropertiesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidReturningArraysOnPropertiesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidReturningArraysOnPropertiesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidReturningArraysOnPropertiesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidReturningArraysOnProperties,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze property declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzePropertySymbol, SymbolKind.Property);
    }

    private void AnalyzePropertySymbol(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;

        // Check if the property's type is an array
        if (propertySymbol.Type.TypeKind == TypeKind.Array)
        {
            var diagnostic = Diagnostic.Create(Rule, propertySymbol.Locations[0], propertySymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}