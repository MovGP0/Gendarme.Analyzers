namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConsiderUsingStaticTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ConsiderUsingStaticTypeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ConsiderUsingStaticTypeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ConsiderUsingStaticTypeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ConsiderUsingStaticType,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Info,
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
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only classes can be static
        if (namedTypeSymbol.TypeKind != TypeKind.Class)
            return;

        // Already static? Skip
        if (namedTypeSymbol.IsStatic)
            return;

        // Check if all members are static
        var hasInstanceMember = false;
        foreach (var member in namedTypeSymbol.GetMembers())
        {
            hasInstanceMember = member switch
            {
                IMethodSymbol { IsStatic: false } method when method.MethodKind != MethodKind.Constructor => true,
                IFieldSymbol { IsStatic: false } => true,
                IPropertySymbol { IsStatic: false } => true,
                IEventSymbol { IsStatic: false } => true,
                _ => hasInstanceMember
            };

            if (hasInstanceMember)
                break;
        }

        if (!hasInstanceMember)
        {
            // All members are static => Suggest making the class static
            var diagnostic = Diagnostic.Create(
                Rule,
                namedTypeSymbol.Locations[0],
                namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}