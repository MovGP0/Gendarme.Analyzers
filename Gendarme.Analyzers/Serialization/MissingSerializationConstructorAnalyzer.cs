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

        if (!namedType.AllInterfaces.Any(i => i.ToDisplayString() == "System.Runtime.Serialization.ISerializable"))
            return;

        var hasSerializationConstructor = namedType.Constructors.Any(c =>
            c.Parameters.Length == 2 &&
            c.Parameters[0].Type.ToDisplayString() == "System.Runtime.Serialization.SerializationInfo" &&
            c.Parameters[1].Type.ToDisplayString() == "System.Runtime.Serialization.StreamingContext" &&
            (namedType.IsSealed ? c.DeclaredAccessibility == Accessibility.Private : c.DeclaredAccessibility == Accessibility.Protected));

        if (!hasSerializationConstructor)
        {
            var diagnostic = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}