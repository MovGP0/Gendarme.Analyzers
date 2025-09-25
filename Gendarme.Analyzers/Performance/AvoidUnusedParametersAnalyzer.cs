namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule is used to ensure that all parameters in a method signature are being used. The rule won't report a defect against the following:
/// <ul>
/// <li>Methods that are referenced by a delegate;</li>
/// <li>Methods used as event handlers;</li>
/// <li>Abstract methods;</li>
/// <li>Virtual or overriden methods;</li>
/// <li>External methods (e.g. p/invokes)</li>
/// </ul>
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public void MethodWithUnusedParameters (IEnumerable enumerable, int x)
/// {
///     foreach (object item in enumerable) {
///         Console.WriteLine (item);
///     }
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public void MethodWithUsedParameters (IEnumerable enumerable)
/// {
///     foreach (object item in enumerable) {
///         Console.WriteLine (item);
///     }
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnusedParametersAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnusedParametersTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnusedParametersMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnusedParametersDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnusedParameters,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<MethodKind> ExcludedMethodKinds = ImmutableHashSet.Create(
        MethodKind.DelegateInvoke,
        MethodKind.ExplicitInterfaceImplementation,
        MethodKind.UserDefinedOperator,
        MethodKind.PropertyGet,
        MethodKind.PropertySet,
        MethodKind.EventAdd,
        MethodKind.EventRemove,
        MethodKind.EventRaise,
        MethodKind.AnonymousFunction);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            var usedParameters = new ConcurrentDictionary<IMethodSymbol, ImmutableHashSet<IParameterSymbol>>(SymbolEqualityComparer.Default);

            startContext.RegisterOperationBlockStartAction(blockStartContext =>
            {
                if (blockStartContext.OwningSymbol is not IMethodSymbol methodSymbol)
                {
                    return;
                }

                if (!ShouldAnalyze(methodSymbol))
                {
                    return;
                }

                var referencedParameters = ImmutableHashSet.CreateBuilder<IParameterSymbol>(SymbolEqualityComparer.Default);

                blockStartContext.RegisterOperationAction(operationContext =>
                {
                    var parameterReference = (IParameterReferenceOperation)operationContext.Operation;
                    referencedParameters.Add(parameterReference.Parameter);
                }, OperationKind.ParameterReference);

                blockStartContext.RegisterOperationBlockEndAction(_ =>
                {
                    usedParameters[methodSymbol] = referencedParameters.ToImmutable();
                });
            });

            startContext.RegisterSymbolAction(symbolContext =>
            {
                var method = (IMethodSymbol)symbolContext.Symbol;

                if (!ShouldAnalyze(method))
                {
                    return;
                }

                if (method.Parameters.Length == 0)
                {
                    return;
                }

                var referenced = usedParameters.TryGetValue(method, out var parameters)
                    ? parameters
                    : ImmutableHashSet<IParameterSymbol>.Empty;

                foreach (var parameter in method.Parameters)
                {
                    if (referenced.Contains(parameter))
                    {
                        continue;
                    }

                    var location = parameter.Locations.FirstOrDefault();
                    if (location is null)
                    {
                        continue;
                    }

                    var diagnostic = Diagnostic.Create(Rule, location, parameter.Name, method.Name);
                    symbolContext.ReportDiagnostic(diagnostic);
                }
            }, SymbolKind.Method);
        });
    }

    private static bool ShouldAnalyze(IMethodSymbol method)
    {
        if (method.IsAbstract || method.IsVirtual || method.IsOverride || method.IsExtern)
        {
            return false;
        }

        if (ExcludedMethodKinds.Contains(method.MethodKind))
        {
            return false;
        }

        if (IsEventHandler(method))
        {
            return false;
        }

        return true;
    }

    private static bool IsEventHandler(IMethodSymbol method)
    {
        if (method.Parameters.Length != 2)
        {
            return false;
        }

        var firstParamType = method.Parameters[0].Type;
        var secondParamType = method.Parameters[1].Type;

        return firstParamType.SpecialType == SpecialType.System_Object &&
               (secondParamType.Name == "EventArgs" || secondParamType.BaseType?.Name == "EventArgs");
    }
}
