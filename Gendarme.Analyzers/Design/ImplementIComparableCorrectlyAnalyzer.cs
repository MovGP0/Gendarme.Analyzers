namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ImplementIComparableCorrectlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.ImplementIComparableCorrectlyTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.ImplementIComparableCorrectlyMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.ImplementIComparableCorrectlyDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ImplementIComparableCorrectly,
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

        // Must implement System.IComparable
        bool implementsIComparable = namedType.AllInterfaces.Any(i =>
            i.ToDisplayString() == "System.IComparable");

        if (!implementsIComparable)
            return;

        // Check that it overrides Equals
        bool overridesEquals = namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m is { Name: nameof(object.Equals), Parameters.Length: 1 } &&
                      m.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                      !m.IsStatic);

        // Check operator ==, !=, <, >
        bool hasOpEq = namedType.GetMembers().OfType<IMethodSymbol>().Any(m => m is { MethodKind: MethodKind.UserDefinedOperator, Name: "op_Equality" });
        bool hasOpNe = namedType.GetMembers().OfType<IMethodSymbol>().Any(m => m is { MethodKind: MethodKind.UserDefinedOperator, Name: "op_Inequality" });
        var missingParts = new List<string>();
        if (!overridesEquals) missingParts.Add("Equals(object)");
        if (!hasOpEq) missingParts.Add("operator ==");
        if (!hasOpNe) missingParts.Add("operator !=");

        if (missingParts.Count > 0)
        {
            var diag = Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name,
                string.Join(", ", missingParts));
            context.ReportDiagnostic(diag);
        }
    }
}