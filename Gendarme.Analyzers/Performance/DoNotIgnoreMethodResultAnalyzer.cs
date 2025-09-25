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

    private static readonly ImmutableHashSet<string> TrackedTypes = ImmutableHashSet.Create(
        "System.String",
        "System.Text.StringBuilder",
        "System.Linq.Enumerable"
        // Add more types as needed
    );

    private static readonly ImmutableHashSet<string> TrackedMethods = ImmutableHashSet.Create(
        "Trim",
        "ToUpper",
        "ToLower",
        "Substring",
        "Replace",
        "Concat",
        "Reverse"
        // Add more methods as needed
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze expression statements
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeExpressionStatement, OperationKind.ExpressionStatement);
    }

    private void AnalyzeExpressionStatement(OperationAnalysisContext context)
    {
        var expressionStatement = (IExpressionStatementOperation)context.Operation;

        if (expressionStatement.Operation is IInvocationOperation invocation)
        {
            var method = invocation.TargetMethod;

            if (TrackedTypes.Contains(method.ContainingType.ToDisplayString()) &&
                TrackedMethods.Contains(method.Name) &&
                method.ReturnType.SpecialType != SpecialType.System_Void)
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), method.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}