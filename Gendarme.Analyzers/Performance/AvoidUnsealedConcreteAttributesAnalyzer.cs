namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnsealedConcreteAttributesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnsealedConcreteAttributesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnsealedConcreteAttributesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnsealedConcreteAttributesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnsealedConcreteAttributes,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeAttributeClass, SymbolKind.NamedType);
    }

    private void AnalyzeAttributeClass(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        // Check if the type is an attribute
        if (!IsAttributeClass(namedType))
            return;

        // Check if the class is not abstract and not sealed
        if (namedType is { IsAbstract: false, IsSealed: false })
        {
            var diagnostic = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsAttributeClass(INamedTypeSymbol type)
    {
        while (type != null)
        {
            if (type.ToDisplayString() == "System.Attribute")
                return true;

            type = type.BaseType;
        }
        return false;
    }
}