namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule fires if a method is called that returns a new instance but that instance is not used.
/// This is a performance problem because it is wasteful to create and collect objects which are never actually used.
/// It may also indicate a logic problem.
/// Note that this rule currently only checks methods within a small number of System types.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public void GetName ()
/// {
///     string name = Console.ReadLine ();
///     // This is a bug: strings are (mostly) immutable so Trim leaves
///     // name untouched and returns a new string.
///     name.Trim ();
///     Console.WriteLine ("Name: {0}", name);
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public void GetName ()
/// {
///     string name = Console.ReadLine ();
///     name = name.Trim ();
///     Console.WriteLine ("Name: {0}", name);
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotIgnoreMethodResultAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotIgnoreMethodResultTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotIgnoreMethodResultMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotIgnoreMethodResultDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotIgnoreMethodResult,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableDictionary<string, ImmutableHashSet<string>> WellKnownMembers =
        new Dictionary<string, ImmutableHashSet<string>>(StringComparer.Ordinal)
        {
            ["System.String"] = ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Trim",
                "TrimStart",
                "TrimEnd",
                "ToUpper",
                "ToLower",
                "Substring",
                "Replace"),
            ["System.Linq.Enumerable"] = ImmutableHashSet.Create(
                StringComparer.Ordinal,
                "Reverse",
                "OrderBy",
                "OrderByDescending",
                "Select",
                "Where",
                "Distinct")
        }.ToImmutableDictionary();

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static compilationStartContext =>
        {
            var trackedMethodsBuilder = ImmutableHashSet.CreateBuilder<IMethodSymbol>(SymbolEqualityComparer.Default);

            foreach (var pair in WellKnownMembers)
            {
                var metadataName = pair.Key;
                var methodNames = pair.Value;

                var typeSymbol = compilationStartContext.Compilation.GetTypeByMetadataName(metadataName);
                if (typeSymbol is null)
                {
                    continue;
                }

                foreach (var member in typeSymbol.GetMembers().OfType<IMethodSymbol>())
                {
                    if (!methodNames.Contains(member.Name) || member.ReturnsVoid)
                    {
                        continue;
                    }

                    trackedMethodsBuilder.Add(member.OriginalDefinition);
                }
            }

            if (trackedMethodsBuilder.Count is 0)
            {
                return;
            }

            var trackedMethods = trackedMethodsBuilder.ToImmutable();

            compilationStartContext.RegisterOperationAction(operationContext =>
            {
                var expressionStatement = (IExpressionStatementOperation)operationContext.Operation;
                if (expressionStatement.Operation is not IInvocationOperation invocation)
                {
                    return;
                }

                var targetMethod = invocation.TargetMethod.OriginalDefinition;
                if (!trackedMethods.Contains(targetMethod))
                {
                    return;
                }

                operationContext.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name));
            }, OperationKind.ExpressionStatement);
        });
    }
}