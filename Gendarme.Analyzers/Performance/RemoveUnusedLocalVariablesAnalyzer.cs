namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule looks for unused local variables inside methods.
/// This can lead to larger code (IL) size and longer JIT time,
/// but note that some optimizing compilers can remove the locals so they won’t
/// be reported even if you can still see them in the source code.
/// This could also be a typo in the source were a value is assigned to the wrong variable.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// bool DualCheck ()
/// {
/// bool b1 = true;
/// bool b2 = CheckDetails ();
///     if (b2) {
///     // typo: a find-replace changed b1 into b2
///     b2 = CheckMoreDetails ();
/// }
/// return b2 && b2;
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// bool DualCheck ()
/// {
/// bool b1 = true;
/// bool b2 = CheckDetails ();
///     if (b2) {
///     b1 = CheckMoreDetails ();
/// }
/// return b1 && b2;
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemoveUnusedLocalVariablesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.RemoveUnusedLocalVariables,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeMethodBody, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodBody(SyntaxNodeAnalysisContext context)
    {
        if (context.SemanticModel is null || context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        var body = methodDeclaration.Body;
        if (body is null)
        {
            return;
        }

        var dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(body);
        if (dataFlowAnalysis is null)
        {
            return;
        }

        var usedVariables = ImmutableHashSet.CreateBuilder<ISymbol>(SymbolEqualityComparer.Default);
        AddSymbols(dataFlowAnalysis.ReadInside);
        AddSymbols(dataFlowAnalysis.WrittenInside);

        foreach (var variable in dataFlowAnalysis.VariablesDeclared)
        {
            if (usedVariables.Contains(variable))
            {
                continue;
            }

            var location = variable.Locations.FirstOrDefault();
            if (location is null || location == Location.None)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, location, variable.Name);
            context.ReportDiagnostic(diagnostic);
        }

        void AddSymbols(ImmutableArray<ISymbol> symbols)
        {
            if (symbols.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (var symbol in symbols)
            {
                usedVariables.Add(symbol);
            }
        }
    }
}
