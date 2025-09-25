namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewInconsistentIdentityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewInconsistentIdentity_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewInconsistentIdentity_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewInconsistentIdentity_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewInconsistentIdentity,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(StartAnalysis);
    }

    private static void StartAnalysis(CompilationStartAnalysisContext context)
    {
        var methodFieldUsages = new ConcurrentDictionary<IMethodSymbol, ImmutableHashSet<ISymbol>>(SymbolEqualityComparer.Default);

        context.RegisterOperationBlockStartAction(blockStartContext =>
        {
            if (blockStartContext.OwningSymbol is not IMethodSymbol methodSymbol)
            {
                return;
            }

            if (methodSymbol.ContainingType is not { } containingType || containingType.TypeKind != TypeKind.Class)
            {
                return;
            }

            if (methodSymbol.MethodKind != MethodKind.Ordinary)
            {
                return;
            }

            var accessedFields = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);

            blockStartContext.RegisterOperationAction(operationContext =>
            {
                var fieldReference = (IFieldReferenceOperation)operationContext.Operation;
                var field = fieldReference.Field;

                if (!field.IsStatic && SymbolEqualityComparer.Default.Equals(field.ContainingType, containingType))
                {
                    accessedFields.Add(field);
                }
            }, OperationKind.FieldReference);

            blockStartContext.RegisterOperationBlockEndAction(_ =>
            {
                methodFieldUsages[methodSymbol] = accessedFields.ToImmutable();
            });
        });

        context.RegisterSymbolAction(symbolContext =>
        {
            var namedTypeSymbol = (INamedTypeSymbol)symbolContext.Symbol;

            var methods = namedTypeSymbol.GetMembers().OfType<IMethodSymbol>().ToImmutableArray();

            var equalsMethods = methods.Where(m => m is { Name: "Equals", Parameters.Length: 1 }).ToList();
            var getHashCodeMethods = methods.Where(m => m is { Name: "GetHashCode", Parameters.Length: 0 }).ToList();
            var compareToMethods = methods.Where(m => m is { Name: "CompareTo", Parameters.Length: 1 }).ToList();

            foreach (var method in equalsMethods)
            {
                if (UsesSameFields(method, getHashCodeMethods, compareToMethods))
                {
                    continue;
                }

                var diagnostic = Diagnostic.Create(Rule, method.Locations[0], namedTypeSymbol.Name);
                symbolContext.ReportDiagnostic(diagnostic);
            }

            ImmutableHashSet<ISymbol> GetFields(IMethodSymbol method)
            {
                return methodFieldUsages.TryGetValue(method, out var fields)
                    ? fields
                    : ImmutableHashSet.Create<ISymbol>(SymbolEqualityComparer.Default);
            }

            bool UsesSameFields(
                IMethodSymbol equalsMethod,
                IEnumerable<IMethodSymbol> getHashCodeCandidates,
                IEnumerable<IMethodSymbol> compareToCandidates)
            {
                var fieldsUsedInEquals = GetFields(equalsMethod);
                var fieldsUsedInGetHashCode = getHashCodeCandidates.SelectMany(GetFields).ToImmutableHashSet(SymbolEqualityComparer.Default);
                var fieldsUsedInCompareTo = compareToCandidates.SelectMany(GetFields).ToImmutableHashSet(SymbolEqualityComparer.Default);

                return fieldsUsedInEquals.SetEquals(fieldsUsedInGetHashCode)
                       && fieldsUsedInEquals.SetEquals(fieldsUsedInCompareTo);
            }
        }, SymbolKind.NamedType);
    }
}
