namespace Gendarme.Analyzers.Serialization;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeserializeOptionalFieldAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DeserializeOptionalFieldTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DeserializeOptionalFieldMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DeserializeOptionalFieldDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DeserializeOptionalField,
        Title,
        MessageFormat,
        Category.Serialization,
        DiagnosticSeverity.Warning,
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
        var optionalFieldAttributeType = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.OptionalFieldAttribute");
        var onDeserializedAttributeType = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.OnDeserializedAttribute");
        var onDeserializingAttributeType = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.OnDeserializingAttribute");

        if (serializableAttributeType is null || optionalFieldAttributeType is null)
        {
            return;
        }

        if (!HasAttribute(namedType, serializableAttributeType))
        {
            return;
        }

        var hasOptionalField = namedType.GetMembers().OfType<IFieldSymbol>()
            .Any(field => HasAttribute(field, optionalFieldAttributeType));

        if (!hasOptionalField)
        {
            return;
        }

        var hasDeserializationMethod = namedType.GetMembers().OfType<IMethodSymbol>()
            .Any(method =>
                (onDeserializedAttributeType is not null && HasAttribute(method, onDeserializedAttributeType)) ||
                (onDeserializingAttributeType is not null && HasAttribute(method, onDeserializingAttributeType)));

        if (hasDeserializationMethod)
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

    private static bool HasAttribute(ISymbol symbol, INamedTypeSymbol attributeType)
    {
        return symbol.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, attributeType));
    }
}
