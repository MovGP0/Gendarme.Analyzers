namespace Gendarme.Analyzers.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidDeepInheritanceTreeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidDeepInheritanceTreeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidDeepInheritanceTreeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidDeepInheritanceTreeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidDeepInheritanceTree,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int MaximumDepth = 4; // This can be made configurable
    private const bool CountExternalDepth = false; // This can be made configurable

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        if (typeSymbol.TypeKind != TypeKind.Class)
            return;

        int depth = 0;
        var baseType = typeSymbol.BaseType;

        while (baseType != null && baseType.SpecialType != SpecialType.System_Object)
        {
            depth++;
            if (!CountExternalDepth && !baseType.ContainingAssembly.Equals(context.Compilation.Assembly))
            {
                break;
            }
            baseType = baseType.BaseType;
        }

        if (depth > MaximumDepth)
        {
            var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], typeSymbol.Name, depth);
            context.ReportDiagnostic(diagnostic);
        }
    }
}