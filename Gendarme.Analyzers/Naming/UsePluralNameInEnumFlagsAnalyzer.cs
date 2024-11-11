using Humanizer;

namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsePluralNameInEnumFlagsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UsePluralNameInEnumFlagsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UsePluralNameInEnumFlagsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UsePluralNameInEnumFlagsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UsePluralNameInEnumFlags,
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

        if (hasFlagsAttribute && name == name.Singularize())
        {
            var diagnostic = Diagnostic.Create(Rule, enumSymbol.Locations[0], name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}