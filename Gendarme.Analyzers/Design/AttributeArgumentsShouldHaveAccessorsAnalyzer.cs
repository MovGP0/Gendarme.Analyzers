namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AttributeArgumentsShouldHaveAccessorsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AttributeArgumentsShouldHaveAccessorsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AttributeArgumentsShouldHaveAccessorsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AttributeArgumentsShouldHaveAccessorsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AttributeArgumentsShouldHaveAccessors,
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
        // We register on named types so we can inspect their constructors
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        // We only care about attribute classes
        var attributeType = context.Compilation.GetTypeByMetadataName("System.Attribute");
        if (attributeType == null || !typeSymbol.InheritsFrom(attributeType))
            return;

        // For each constructor, check if parameters are exposed
        foreach (var ctor in typeSymbol.Constructors)
        {
            if (ctor.DeclaredAccessibility == Accessibility.Public)
            {
                foreach (var parameter in ctor.Parameters)
                {
                    // Expected property name
                    var expectedPropName = parameter.Name.Substring(0, 1).ToUpperInvariant() +
                                           parameter.Name.Substring(1);

                    // Check if property with matching name exists
                    var hasProperty = typeSymbol
                        .GetMembers()
                        .OfType<IPropertySymbol>()
                        .Any(p => p.Name == expectedPropName);

                    if (!hasProperty)
                    {
                        // Report diagnostic on the ctor location or parameter location
                        var diagnostic = Diagnostic.Create(Rule, ctor.Locations[0],
                            typeSymbol.Name, parameter.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}

// Simple helper extension to check inheritance
internal static class SymbolExtensions
{
    public static bool InheritsFrom(this ITypeSymbol symbol, ITypeSymbol baseType)
    {
        while (symbol != null)
        {
            if (SymbolEqualityComparer.Default.Equals(symbol.BaseType, baseType))
            {
                return true;
            }
            symbol = symbol.BaseType;
        }
        return false;
    }
}