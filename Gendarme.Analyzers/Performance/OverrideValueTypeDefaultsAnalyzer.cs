namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OverrideValueTypeDefaultsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.OverrideValueTypeDefaultsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.OverrideValueTypeDefaultsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.OverrideValueTypeDefaultsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.OverrideValueTypeDefaults,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types (structs)
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeValueType, SymbolKind.NamedType);
    }

    private static void AnalyzeValueType(SymbolAnalysisContext context)
    {
        var structSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze structs (value types), excluding enums
        if (structSymbol.TypeKind is not TypeKind.Struct)
            return;

        // Check if Equals(object) and GetHashCode() are overridden
        var overridesEquals = structSymbol.GetMembers().OfType<IMethodSymbol>()
            .Any(m => m is { Name: "Equals", Parameters.Length: 1 } &&
                      m.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                      m.IsOverride);

        var overridesGetHashCode = structSymbol.GetMembers().OfType<IMethodSymbol>()
            .Any(m => m is { Name: "GetHashCode", Parameters.Length: 0, IsOverride: true });

        if (!overridesEquals || !overridesGetHashCode)
        {
            var diagnostic = Diagnostic.Create(Rule, structSymbol.Locations[0], structSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}