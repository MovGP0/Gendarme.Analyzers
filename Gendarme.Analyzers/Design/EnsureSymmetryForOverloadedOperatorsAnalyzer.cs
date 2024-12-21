namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnsureSymmetryForOverloadedOperatorsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.EnsureSymmetryForOverloadedOperatorsTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.EnsureSymmetryForOverloadedOperatorsMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.EnsureSymmetryForOverloadedOperatorsDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.EnsureSymmetryForOverloadedOperators,
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
        var operatorMethods = namedType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.UserDefinedOperator);

        // Pairs to check: +/-, ==/!=, <,>, <=,>=, etc.
        // For simplicity, we'll demonstrate just +/-
        var hasPlus = operatorMethods.Any(m => m.MetadataName == "op_Addition");
        var hasMinus = operatorMethods.Any(m => m.MetadataName == "op_Subtraction");

        if (hasPlus && !hasMinus)
        {
            var diag = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name, "+");
            context.ReportDiagnostic(diag);
        }
        if (hasMinus && !hasPlus)
        {
            var diag = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name, "-");
            context.ReportDiagnostic(diag);
        }

        // You can expand the above pattern for all relevant operators
    }
}