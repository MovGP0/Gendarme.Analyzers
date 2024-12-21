namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OperatorEqualsShouldBeOverloadedAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.OperatorEqualsShouldBeOverloadedTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.OperatorEqualsShouldBeOverloadedMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.OperatorEqualsShouldBeOverloadedDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.OperatorEqualsShouldBeOverloaded,
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
        var methods = namedType.GetMembers().OfType<IMethodSymbol>().ToList();

        // 1. If it's a value type that overrides Equals, but no operator ==
        bool isValueType = namedType.IsValueType;
        bool overridesEquals = methods.Any(m =>
            m is { Name: nameof(object.Equals), Parameters.Length: 1 } &&
            m.Parameters[0].Type.SpecialType == SpecialType.System_Object);

        bool hasOpEq = methods.Any(m => m is { MethodKind: MethodKind.UserDefinedOperator, Name: "op_Equality" });

        // 2. or if it overloads + or - but doesn't overload ==
        bool hasOpAdd = methods.Any(m => m is { MethodKind: MethodKind.UserDefinedOperator, Name: "op_Addition" });
        bool hasOpSub = methods.Any(m => m is { MethodKind: MethodKind.UserDefinedOperator, Name: "op_Subtraction" });

        bool shouldHaveOpEq = (isValueType && overridesEquals) || hasOpAdd || hasOpSub;
        if (shouldHaveOpEq && !hasOpEq)
        {
            var diag = Diagnostic.Create(Rule, namedType.Locations.FirstOrDefault(), namedType.Name);
            context.ReportDiagnostic(diag);
        }
    }
}