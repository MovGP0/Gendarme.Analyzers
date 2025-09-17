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
        // Analyze named types
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        if (!namedType.GetAttributes().Any(attr => attr.AttributeClass.ToDisplayString() == "System.SerializableAttribute"))
            return;

        var hasOptionalField = namedType.GetMembers().OfType<IFieldSymbol>()
            .Any(f => f.GetAttributes().Any(attr => attr.AttributeClass.ToDisplayString() == "System.Runtime.Serialization.OptionalFieldAttribute"));

        if (!hasOptionalField)
            return;

        var hasDeserializationMethod = namedType.GetMembers().OfType<IMethodSymbol>()
            .Any(m => m.GetAttributes().Any(attr =>
                attr.AttributeClass.ToDisplayString() == "System.Runtime.Serialization.OnDeserializedAttribute" ||
                attr.AttributeClass.ToDisplayString() == "System.Runtime.Serialization.OnDeserializingAttribute"));

        if (!hasDeserializationMethod)
        {
            var diagnostic = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}