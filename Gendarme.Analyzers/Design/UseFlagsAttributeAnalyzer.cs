namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseFlagsAttributeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.UseFlagsAttributeTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.UseFlagsAttributeMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.UseFlagsAttributeDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseFlagsAttribute,
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
        if (namedType.TypeKind != TypeKind.Enum)
            return;

        // If the enum already has [Flags], skip
        bool hasFlags = namedType.GetAttributes()
            .Any(a => a.AttributeClass?.Name == nameof(System.FlagsAttribute));

        if (hasFlags)
            return;

        // Very naive check: if any fields are powers of two and more than one field => suggests bitmask usage
        var fields = namedType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f is { HasConstantValue: true, ConstantValue: int }).ToList();

        int countBitfields = 0;
        foreach (var field in fields)
        {
            int val = (int)field.ConstantValue;
            // skip the '0' field or negative or weird values
            if (val > 0 && (val & (val - 1)) == 0) // power-of-two check
            {
                countBitfields++;
                if (countBitfields > 1)
                    break;
            }
        }

        // If we found multiple power-of-two fields, it's likely a bitmask
        if (countBitfields > 1)
        {
            var diag = Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name);
            context.ReportDiagnostic(diag);
        }
    }
}