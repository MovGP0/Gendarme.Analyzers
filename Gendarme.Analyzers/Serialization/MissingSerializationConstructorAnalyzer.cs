namespace Gendarme.Analyzers.Serialization;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingSerializationConstructorAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MissingSerializationConstructorTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MissingSerializationConstructorMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MissingSerializationConstructorDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MissingSerializationConstructor,
        Title,
        MessageFormat,
        Category.Serialization,
        DiagnosticSeverity.Error,
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
        if (context.Symbol is not INamedTypeSymbol namedType)
        {
            return;
        }

        var serializableAttributeType = context.Compilation.GetTypeByMetadataName("System.SerializableAttribute");
        var iSerializableType = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.ISerializable");
        var serializationInfoType = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.SerializationInfo");
        var streamingContextType = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.StreamingContext");

        if (serializableAttributeType is null ||
            iSerializableType is null ||
            serializationInfoType is null ||
            streamingContextType is null)
        {
            return;
        }

        if (!namedType.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, serializableAttributeType)))
        {
            return;
        }

        if (!namedType.AllInterfaces.Any(@interface => SymbolEqualityComparer.Default.Equals(@interface, iSerializableType)))
        {
            return;
        }

        var hasSerializationConstructor = namedType.Constructors.Any(constructor =>
            constructor.Parameters.Length == 2 &&
            SymbolEqualityComparer.Default.Equals(constructor.Parameters[0].Type, serializationInfoType) &&
            SymbolEqualityComparer.Default.Equals(constructor.Parameters[1].Type, streamingContextType) &&
            (namedType.IsSealed ? constructor.DeclaredAccessibility == Accessibility.Private : constructor.DeclaredAccessibility == Accessibility.Protected));

        if (hasSerializationConstructor)
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
