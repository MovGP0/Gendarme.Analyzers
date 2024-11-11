namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotPrefixValuesWithEnumNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotPrefixValuesWithEnumNameTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotPrefixValuesWithEnumNameMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotPrefixValuesWithEnumNameDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotPrefixValuesWithEnumName,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeEnum, SymbolKind.NamedType);
    }

    private static void AnalyzeEnum(SymbolAnalysisContext context)
    {
        var enumSymbol = (INamedTypeSymbol)context.Symbol;
        if (enumSymbol.TypeKind != TypeKind.Enum)
            return;

        var enumName = enumSymbol.Name;

        foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.Name.StartsWith(enumName))
            {
                var diagnostic = Diagnostic.Create(Rule, member.Locations[0], member.Name, enumName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}