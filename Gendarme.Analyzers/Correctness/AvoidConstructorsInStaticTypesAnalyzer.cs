namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidConstructorsInStaticTypesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.AvoidConstructorsInStaticTypes_Title;
    private static readonly LocalizableString MessageFormat = Strings.AvoidConstructorsInStaticTypes_Message;
    private static readonly LocalizableString Description = Strings.AvoidConstructorsInStaticTypes_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidConstructorsInStaticTypes,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.IsStatic)
        {
            return;
        }

        var hasStaticMembersOnly = namedTypeSymbol.GetMembers().All(member =>
            member.Kind == SymbolKind.Method && ((IMethodSymbol)member).IsStatic ||
            member.Kind == SymbolKind.Property && ((IPropertySymbol)member).IsStatic ||
            member.Kind == SymbolKind.Field && ((IFieldSymbol)member).IsStatic ||
            member.Kind == SymbolKind.Event && ((IEventSymbol)member).IsStatic);

        if (!hasStaticMembersOnly)
        {
            return;
        }

        foreach (var constructor in namedTypeSymbol.Constructors)
        {
            if (constructor.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected)
            {
                var diagnostic = Diagnostic.Create(Rule, constructor.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
