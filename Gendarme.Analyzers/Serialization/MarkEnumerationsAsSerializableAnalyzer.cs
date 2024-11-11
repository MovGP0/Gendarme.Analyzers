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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        if (namedType.TypeKind != TypeKind.Enum)
            return;

        if (!namedType.GetAttributes().Any(attr => attr.AttributeClass.ToDisplayString() == "System.SerializableAttribute"))
        {
            var diagnostic = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}