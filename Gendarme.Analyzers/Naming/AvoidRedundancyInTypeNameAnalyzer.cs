namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidRedundancyInTypeNameAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidRedundancyInTypeNameTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidRedundancyInTypeNameMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidRedundancyInTypeNameDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidRedundancyInTypeName,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        var namespaceParts = typeSymbol.ContainingNamespace?.ToDisplayString().Split('.');

        if (namespaceParts == null || namespaceParts.Length == 0)
            return;

        var lastNamespacePart = namespaceParts.Last();
        var typeName = typeSymbol.Name;

        if (typeName.StartsWith(lastNamespacePart) && !IsAmbiguous(typeSymbol, lastNamespacePart))
        {
            var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], typeName, lastNamespacePart);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsAmbiguous(INamedTypeSymbol typeSymbol, string proposedName)
    {
        var containingNamespace = typeSymbol.ContainingNamespace;
        var typesWithSameName = containingNamespace.GetMembers(proposedName).OfType<INamedTypeSymbol>();
        return typesWithSameName.Any(t => !t.Equals(typeSymbol));
    }
}