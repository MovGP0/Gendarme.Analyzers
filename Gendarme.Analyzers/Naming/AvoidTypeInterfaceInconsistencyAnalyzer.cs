namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidTypeInterfaceInconsistencyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidTypeInterfaceInconsistencyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidTypeInterfaceInconsistencyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidTypeInterfaceInconsistencyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidTypeInterfaceInconsistency,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description,
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            var interfaceTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
            var classTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                var symbol = (INamedTypeSymbol)symbolContext.Symbol;
                if (symbol.TypeKind == TypeKind.Interface)
                {
                    interfaceTypes.Add(symbol);
                }
                else if (symbol.TypeKind == TypeKind.Class)
                {
                    classTypes.Add(symbol);
                }
            }, SymbolKind.NamedType);

            startContext.RegisterCompilationEndAction(endContext =>
            {
                foreach (var interfaceType in interfaceTypes)
                {
                    var typeNameWithoutI = interfaceType.Name.TrimStart('I');
                    var matchingClass = classTypes.FirstOrDefault(c => c.Name == typeNameWithoutI && c.ContainingNamespace.Equals(interfaceType.ContainingNamespace, SymbolEqualityComparer.Default));

                    if (matchingClass != null && !matchingClass.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, interfaceType)))
                    {
                        var diagnostic = Diagnostic.Create(Rule, matchingClass.Locations[0], matchingClass.Name, interfaceType.Name);
                        endContext.ReportDiagnostic(diagnostic);
                    }
                }
            });
        });
    }
}
