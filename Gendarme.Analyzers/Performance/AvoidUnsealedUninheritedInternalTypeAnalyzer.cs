namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnsealedUninheritedInternalTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnsealedUninheritedInternalTypeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnsealedUninheritedInternalTypeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnsealedUninheritedInternalTypeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnsealedUninheritedInternalType,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description,
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private static void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        var derivedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        context.RegisterSymbolAction(symbolContext =>
        {
            var namedType = (INamedTypeSymbol)symbolContext.Symbol;

            if (namedType.BaseType is { SpecialType: SpecialType.System_Object })
            {
                derivedTypes.Add(namedType.BaseType);
            }
        }, SymbolKind.NamedType);

        context.RegisterCompilationEndAction(compilationContext =>
        {
            var compilation = compilationContext.Compilation;

            var internalTypes = GetAllNamespaceTypes(compilation.Assembly.GlobalNamespace)
                .Where(t => t is
                {
                    IsSealed: false,
                    IsAbstract: false,
                    TypeKind: TypeKind.Class,
                    DeclaredAccessibility: Accessibility.Internal
                });

            foreach (var type in internalTypes)
            {
                if (derivedTypes.Contains(type))
                {
                    continue;
                }

                var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.Name);
                compilationContext.ReportDiagnostic(diagnostic);
            }
        });
    }

    private static IEnumerable<INamedTypeSymbol> GetAllNamespaceTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol nestedNamespace:
                {
                    foreach (var type in GetAllNamespaceTypes(nestedNamespace))
                        yield return type;
                    break;
                }
                case INamedTypeSymbol namedType:
                    yield return namedType;
                    break;
            }
        }
    }
}