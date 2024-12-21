namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ImplementEqualsAndGetHashCodeInPairAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.ImplementEqualsAndGetHashCodeInPairTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.ImplementEqualsAndGetHashCodeInPairMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.ImplementEqualsAndGetHashCodeInPairDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ImplementEqualsAndGetHashCodeInPair,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (namedType.TypeKind == TypeKind.Interface) return;

        bool overridesEquals = namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m is { Name: nameof(object.Equals), Parameters.Length: 1 } &&
                      m.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                      !m.IsStatic);

        bool overridesGetHashCode = namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m is { Name: nameof(object.GetHashCode), Parameters.Length: 0, IsStatic: false });

        // If type overrides only one of them, report a diagnostic
        if (overridesEquals && !overridesGetHashCode)
        {
            var diag = Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name,
                nameof(object.Equals),
                nameof(object.GetHashCode));
            context.ReportDiagnostic(diag);
        }
        else if (!overridesEquals && overridesGetHashCode)
        {
            var diag = Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name,
                nameof(object.GetHashCode),
                nameof(object.Equals));
            context.ReportDiagnostic(diag);
        }
    }
}