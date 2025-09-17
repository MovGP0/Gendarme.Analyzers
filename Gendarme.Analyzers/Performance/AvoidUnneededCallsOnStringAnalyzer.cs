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

    private static readonly ImmutableHashSet<string> UnneededMethods = ImmutableHashSet.Create("ToString", "Clone", "Substring");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method invocations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;

        if (invocation.Instance != null &&
            invocation.Instance.Type.SpecialType == SpecialType.System_String &&
            UnneededMethods.Contains(invocation.TargetMethod.Name))
        {
            // Special handling for Substring(0)
            if (invocation.TargetMethod.Name == "Substring")
            {
                if (invocation.Arguments is [{ Value.ConstantValue: { HasValue: true, Value: int and 0 } }])
                {
                    var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name, invocation.Instance.Syntax.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), invocation.TargetMethod.Name, invocation.Instance.Syntax.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}