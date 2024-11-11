using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferCharOverloadAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.PreferCharOverloadTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.PreferCharOverloadMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.PreferCharOverloadDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferCharOverload,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<string> TargetMethods = ImmutableHashSet.Create(
        "IndexOf",
        "LastIndexOf",
        "StartsWith",
        "EndsWith",
        "Contains",
        "Split",
        "Replace"
    );

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

        if (invocation.Instance == null || invocation.Instance.Type.SpecialType != SpecialType.System_String)
            return;

        var methodName = invocation.TargetMethod.Name;

        if (!TargetMethods.Contains(methodName))
            return;

        if (invocation.Arguments.Length == 1)
        {
            var argument = invocation.Arguments[0];
            if (argument.Value.ConstantValue.HasValue && argument.Value.Type.SpecialType == SpecialType.System_String)
            {
                var value = argument.Value.ConstantValue.Value as string;
                if (!string.IsNullOrEmpty(value) && value.Length == 1)
                {
                    // Suggest using char overload
                    var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), methodName, "string");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}