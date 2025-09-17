namespace Gendarme.Analyzers.Serialization;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarkEnumerationsAsSerializableAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MarkEnumerationsAsSerializableTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MarkEnumerationsAsSerializableMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MarkEnumerationsAsSerializableDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MarkEnumerationsAsSerializable,
        Title,
        MessageFormat,
        Category.Serialization,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol namedType || namedType.TypeKind != TypeKind.Enum)
        {
            return;
        }

        var serializableAttributeType = context.Compilation.GetTypeByMetadataName("System.SerializableAttribute");
        var hasSerializableAttribute = namedType.GetAttributes().Any(attribute =>
            serializableAttributeType is not null
                ? SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serializableAttributeType)
                : attribute.AttributeClass?.ToDisplayString() == "System.SerializableAttribute");

        if (hasSerializableAttribute)
        {
            return;
        }

        var location = namedType.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, location, namedType.Name);
        context.ReportDiagnostic(diagnostic);
    }
}
