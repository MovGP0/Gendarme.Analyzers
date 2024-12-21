namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingAttributeUsageOnCustomAttributeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.MissingAttributeUsageOnCustomAttributeTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.MissingAttributeUsageOnCustomAttributeMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.MissingAttributeUsageOnCustomAttributeDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MissingAttributeUsageOnCustomAttribute,
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

        // Check if it inherits from System.Attribute
        if (namedType.BaseType == null) return;
        if (namedType.BaseType.ToDisplayString() != "System.Attribute") return;

        // Check if the type has the [AttributeUsage] attribute
        bool hasAttributeUsage = namedType.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == "System.AttributeUsageAttribute");

        if (!hasAttributeUsage)
        {
            var diag = Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name);
            context.ReportDiagnostic(diag);
        }
    }
}