namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FlagsShouldNotDefineAZeroValueAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.FlagsShouldNotDefineAZeroValueTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.FlagsShouldNotDefineAZeroValueMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.FlagsShouldNotDefineAZeroValueDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.FlagsShouldNotDefineAZeroValue,
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
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (namedType.TypeKind != TypeKind.Enum)
            return;

        // Check if it has [Flags]
        bool isFlags = namedType.GetAttributes()
            .Any(a => a.AttributeClass?.Name == nameof(FlagsAttribute));

        if (!isFlags)
            return;

        // If it has a zero value, we raise a diagnostic
        var zeroField = namedType.GetMembers()
            .OfType<IFieldSymbol>()
            .FirstOrDefault(f => f is { HasConstantValue: true, ConstantValue: int and 0 });

        if (zeroField != null)
        {
            // Report on the enum identifier to match expectations
            var location = namedType.Locations.FirstOrDefault();
            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}