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
        StringComparer.Ordinal,
        "IndexOf",
        "LastIndexOf",
        "StartsWith",
        "EndsWith",
        "Contains",
        "Split",
        "Replace");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        if (invocation.Instance is not { Type.SpecialType: SpecialType.System_String })
        {
            return;
        }

        var targetMethod = invocation.TargetMethod;
        if (targetMethod is null || !TargetMethods.Contains(targetMethod.Name))
        {
            return;
        }

        if (invocation.Arguments.Length != 1)
        {
            return;
        }

        var argument = invocation.Arguments[0];
        var argumentValue = argument.Value;
        if (argumentValue is null || argumentValue.Type?.SpecialType != SpecialType.System_String)
        {
            return;
        }

        if (argumentValue.ConstantValue is not { HasValue: true, Value: string stringValue })
        {
            return;
        }

        if (stringValue.Length != 1)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), targetMethod.Name, "string");
        context.ReportDiagnostic(diagnostic);
    }
}
