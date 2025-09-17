using Humanizer;

namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseSingularNameInEnumsUnlessAreFlagsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseSingularNameInEnumsUnlessAreFlagsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseSingularNameInEnumsUnlessAreFlagsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseSingularNameInEnumsUnlessAreFlagsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseSingularNameInEnumsUnlessAreFlags,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeEnum, SymbolKind.NamedType);
    }

    private static void AnalyzeEnum(SymbolAnalysisContext context)
    {
        var enumSymbol = (INamedTypeSymbol)context.Symbol;
        if (enumSymbol.TypeKind != TypeKind.Enum)
            return;

        var hasFlagsAttribute = enumSymbol.GetAttributes().Any(attr => attr.AttributeClass?.Name == "FlagsAttribute");
        var name = enumSymbol.Name;

        if (!hasFlagsAttribute && name == name.Pluralize())
        {
            var diagnostic = Diagnostic.Create(Rule, enumSymbol.Locations[0], name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}