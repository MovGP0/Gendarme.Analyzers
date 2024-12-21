namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidPropertiesWithoutGetAccessorAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidPropertiesWithoutGetAccessorTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidPropertiesWithoutGetAccessorMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidPropertiesWithoutGetAccessorDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidPropertiesWithoutGetAccessor,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;

        // Externally visible?
        if (!propertySymbol.IsExternallyVisible())
            return;

        // If it has a set accessor but no get accessor => warning
        if (propertySymbol is { SetMethod: not null, GetMethod: null })
        {
            var diagnostic = Diagnostic.Create(Rule, propertySymbol.Locations[0],
                propertySymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}

internal static class AccessibilityExtensions
{
    public static bool IsExternallyVisible(this ISymbol symbol)
    {
        return symbol.DeclaredAccessibility switch
        {
            Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal => true,
            _ => false
        };
    }
}