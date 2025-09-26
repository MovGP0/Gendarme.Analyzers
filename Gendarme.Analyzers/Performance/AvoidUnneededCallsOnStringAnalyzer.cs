namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule detects when some methods, like <c>Clone()</c>, <c>Substring(0)</c>, <c>ToString()</c> or <c>ToString(IFormatProvider)</c>,
/// are being called on a string instance.<br/>
/// Since these calls all return the original string they don’t do anything useful
/// and should be carefully reviewed to see if they are working as intended and, if they are, the method call can be removed.
/// </summary>
/// <example>
/// Bad example:
/// <code language="csharp">
/// public void PrintName (string name)
/// {
///     Console.WriteLine ("Name: {0}", name.ToString ());
/// }
/// </code>
/// Good example:
/// <code language="csharp">
/// public void PrintName (string name)
/// {
///     Console.WriteLine ("Name: {0}", name);
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnneededCallsOnStringAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnneededCallsOnStringTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnneededCallsOnStringMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnneededCallsOnStringDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnneededCallsOnString,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<string> UnneededMethods = ImmutableHashSet.Create(StringComparer.Ordinal, "ToString", "Clone", "Substring");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var instance = invocation.Instance;
        if (instance is not { Type.SpecialType: SpecialType.System_String })
        {
            return;
        }

        var targetMethod = invocation.TargetMethod;
        if (!UnneededMethods.Contains(targetMethod.Name))
        {
            return;
        }

        var syntax = instance.Syntax;

        if (targetMethod.Name == "Substring")
        {
            if (invocation.Arguments is [{ Value.ConstantValue: { HasValue: true, Value: int and 0 } }])
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), targetMethod.Name, syntax.ToString()));
            }

            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), targetMethod.Name, syntax.ToString()));
    }
}
