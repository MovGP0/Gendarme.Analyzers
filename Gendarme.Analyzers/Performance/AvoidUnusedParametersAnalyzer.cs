namespace Gendarme.Analyzers.Performance;

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

    private static readonly ImmutableHashSet<string> ExcludedMethodKinds = ImmutableHashSet.Create(
        "DelegateInvoke",
        "ExplicitInterfaceImplementation",
        "Operator",
        "Accessor",
        "AnonymousFunction"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method symbols
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        // Skip abstract, virtual, override, extern methods
        if (method.IsAbstract || method.IsVirtual || method.IsOverride || method.IsExtern)
            return;

        // Skip methods with special kinds
        if (ExcludedMethodKinds.Contains(method.MethodKind.ToString()))
            return;

        // Skip event handlers (methods with specific signatures)
        if (IsEventHandler(method))
            return;

        var dataFlowAnalysis = (
                from syntaxRef in method.DeclaringSyntaxReferences
                let syntax = syntaxRef.GetSyntax(context.CancellationToken)
                let semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree)
                select semanticModel.AnalyzeDataFlow(syntax))
            .FirstOrDefault();

        if (dataFlowAnalysis == null)
            return;

        var usedParameters = dataFlowAnalysis.ReadInside
            .Union(dataFlowAnalysis.WrittenInside, SymbolEqualityComparer.Default)
            .OfType<IParameterSymbol>()
            .Select(p => p.Name)
            .ToImmutableHashSet();

        foreach (var parameter in method.Parameters)
        {
            if (usedParameters.Contains(parameter.Name))
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, parameter.Locations[0], parameter.Name, method.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsEventHandler(IMethodSymbol method)
    {
        if (method.Parameters.Length != 2) return false;
        var firstParamType = method.Parameters[0].Type;
        var secondParamType = method.Parameters[1].Type;

        if (firstParamType.SpecialType == SpecialType.System_Object &&
            (secondParamType.Name == "EventArgs" || secondParamType.BaseType?.Name == "EventArgs"))
        {
            return true;
        }
        return false;
    }
}