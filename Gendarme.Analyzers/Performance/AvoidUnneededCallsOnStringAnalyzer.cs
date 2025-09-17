using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

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
        if (targetMethod is null || !UnneededMethods.Contains(targetMethod.Name))
        {
            return;
        }

        var syntax = instance.Syntax;
        if (syntax is null)
        {
            return;
        }

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

