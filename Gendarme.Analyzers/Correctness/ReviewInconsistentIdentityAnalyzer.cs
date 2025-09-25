namespace Gendarme.Analyzers.Correctness;

/// <summary>
/// This rule checks to see if a type manages its identity in a consistent way. It checks:
/// <ul>
/// <li>Equals methods, relational operators and CompareTo must either use the same set of fields and properties or call a helper method.</li>
/// <li>GetHashCode must use the same or a subset of the fields used by the equality methods or call a helper method.</li>
/// <li>Clone must use the same or a superset of the fields used by the equality methods or call a helper method.</li>
/// </ul>
/// </summary>
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
        description: Description,
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(StartAnalysis);
    }

    private static void StartAnalysis(CompilationStartAnalysisContext context)
    {
        var methodMemberUsages = new ConcurrentDictionary<IMethodSymbol, ImmutableHashSet<ISymbol>>(SymbolEqualityComparer.Default);
        var methodSameTypeCalls = new ConcurrentDictionary<IMethodSymbol, ImmutableHashSet<IMethodSymbol>>(SymbolEqualityComparer.Default);
        var candidateTypes = new ConcurrentDictionary<INamedTypeSymbol, byte>(SymbolEqualityComparer.Default);

        context.RegisterOperationBlockStartAction(blockStartContext =>
        {
            if (blockStartContext.OwningSymbol is not IMethodSymbol methodSymbol)
                return;

            var containingType = methodSymbol.ContainingType;
            if (containingType is null || containingType.TypeKind != TypeKind.Class)
                return;

            candidateTypes[containingType] = 0;

            var members = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);
            var callees = ImmutableHashSet.CreateBuilder<IMethodSymbol>(SymbolEqualityComparer.Default);

            blockStartContext.RegisterOperationAction(operationContext =>
            {
                var field = ((IFieldReferenceOperation)operationContext.Operation).Field;
                if (!field.IsStatic && SymbolEqualityComparer.Default.Equals(field.ContainingType, containingType))
                {
                    members.Add(field);
                }
            }, OperationKind.FieldReference);

            blockStartContext.RegisterOperationAction(operationContext =>
            {
                var property = ((IPropertyReferenceOperation)operationContext.Operation).Property;
                if (!property.IsStatic && SymbolEqualityComparer.Default.Equals(property.ContainingType, containingType))
                {
                    members.Add(property);
                }
            }, OperationKind.PropertyReference);

            blockStartContext.RegisterOperationAction(operationContext =>
            {
                var target = ((IInvocationOperation)operationContext.Operation).TargetMethod;
                if (SymbolEqualityComparer.Default.Equals(target.ContainingType, containingType))
                {
                    callees.Add(target);
                }
            }, OperationKind.Invocation);

            blockStartContext.RegisterOperationBlockEndAction(_ =>
            {
                methodMemberUsages[methodSymbol] = members.ToImmutable();
                methodSameTypeCalls[methodSymbol] = callees.ToImmutable();
            });
        });

        context.RegisterCompilationEndAction(compilationEndContext =>
        {
            foreach (var type in candidateTypes.Keys)
            {
                var methods = type.GetMembers().OfType<IMethodSymbol>().ToImmutableArray();

                var equalsMethods = methods.Where(m => m.Name == "Equals" && m.Parameters.Length == 1).ToImmutableArray();
                var compareToMethods = methods.Where(m => m.Name == "CompareTo" && m.Parameters.Length == 1).ToImmutableArray();
                var operatorEquals = methods.Where(m => m.MethodKind == MethodKind.UserDefinedOperator && m.Name == "op_Equality").ToImmutableArray();
                var operatorNotEquals = methods.Where(m => m.MethodKind == MethodKind.UserDefinedOperator && m.Name == "op_Inequality").ToImmutableArray();
                var getHashCodeMethods = methods.Where(m => m.Name == "GetHashCode" && m.Parameters.Length == 0).ToImmutableArray();
                var cloneMethods = methods.Where(m => m.Name == "Clone" && m.Parameters.Length == 0).ToImmutableArray();

                var aggregatedCache = new Dictionary<IMethodSymbol, ImmutableHashSet<ISymbol>>(SymbolEqualityComparer.Default);
                ImmutableHashSet<ISymbol> AggregateMembers(IMethodSymbol method)
                {
                    if (aggregatedCache.TryGetValue(method, out var cached))
                        return cached;

                    var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
                    var stack = new Stack<IMethodSymbol>();
                    stack.Push(method);
                    var builder = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);

                    while (stack.Count > 0)
                    {
                        var current = stack.Pop();
                        if (!visited.Add(current))
                            continue;

                        if (methodMemberUsages.TryGetValue(current, out var currentMembers))
                        {
                            builder.UnionWith(currentMembers);
                        }

                        if (methodSameTypeCalls.TryGetValue(current, out var calls))
                        {
                            foreach (var callee in calls)
                            {
                                stack.Push(callee);
                            }
                        }
                    }

                    var result = builder.ToImmutable();
                    aggregatedCache[method] = result;
                    return result;
                }

                var equalityGroup = equalsMethods.Concat(compareToMethods).Concat(operatorEquals).Concat(operatorNotEquals).ToImmutableArray();
                var equalitySets = equalityGroup.Select(AggregateMembers).ToList();

                bool EqualitySetsConsistent()
                {
                    if (equalitySets.Count <= 1)
                        return true;
                    var first = equalitySets[0];
                    for (int i = 1; i < equalitySets.Count; i++)
                    {
                        if (!first.SetEquals(equalitySets[i]))
                            return false;
                    }
                    return true;
                }

                if (!EqualitySetsConsistent())
                {
                    compilationEndContext.ReportDiagnostic(Diagnostic.Create(Rule, Location.None, type.Name));
                    continue;
                }

                var equalitySet = equalitySets.FirstOrDefault() ?? ImmutableHashSet<ISymbol>.Empty;

                foreach (var ghc in getHashCodeMethods)
                {
                    var set = AggregateMembers(ghc);
                    if (!set.IsSubsetOf(equalitySet))
                    {
                        compilationEndContext.ReportDiagnostic(Diagnostic.Create(Rule, Location.None, type.Name));
                        goto NextType;
                    }
                }

                foreach (var clone in cloneMethods)
                {
                    var set = AggregateMembers(clone);
                    if (!equalitySet.IsSubsetOf(set))
                    {
                        compilationEndContext.ReportDiagnostic(Diagnostic.Create(Rule, Location.None, type.Name));
                        goto NextType;
                    }
                }

            NextType:
                ;
            }
        });
    }
}
