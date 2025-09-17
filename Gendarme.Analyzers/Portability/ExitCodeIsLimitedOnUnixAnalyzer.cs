using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Portability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExitCodeIsLimitedOnUnixAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ExitCodeIsLimitedOnUnixTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ExitCodeIsLimitedOnUnixMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ExitCodeIsLimitedOnUnixDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ExitCodeIsLimitedOnUnix,
        Title,
        MessageFormat,
        Category.Portability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method returns, assignments, and invocations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeReturnStatement, OperationKind.Return);
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeReturnStatement(OperationAnalysisContext context)
    {
        var returnOperation = (IReturnOperation)context.Operation;

        if (returnOperation.SemanticModel.GetEnclosingSymbol(returnOperation.Syntax.SpanStart) is IMethodSymbol { Name: "Main", ReturnType.SpecialType: SpecialType.System_Int32 })
        {
            // Check if return value is a constant out of range 0-255
            var returnedValue = returnOperation.ReturnedValue;
            if (returnedValue is { ConstantValue: { HasValue: true, Value: int and (< 0 or > 255) } })
            {
                var diagnostic = Diagnostic.Create(Rule, returnOperation.Syntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeAssignment(OperationAnalysisContext context)
    {
        var assignmentOperation = (ISimpleAssignmentOperation)context.Operation;

        if (assignmentOperation.Target is IPropertyReferenceOperation propertyReference)
        {
            if (propertyReference.Property.Name == "ExitCode" && propertyReference.Property.ContainingType.Name == "Environment")
            {
                // Check if assigned value is a constant out of range 0-255
                var assignedValue = assignmentOperation.Value;
                if (assignedValue is { ConstantValue: { HasValue: true, Value: int and (< 0 or > 255) } })
                {
                    var diagnostic = Diagnostic.Create(Rule, assignmentOperation.Syntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocationOperation = (IInvocationOperation)context.Operation;

        if (invocationOperation.TargetMethod.Name == "Exit" && invocationOperation.TargetMethod.ContainingType.Name == "Environment")
        {
            if (invocationOperation.Arguments is [{ Value.ConstantValue: { HasValue: true, Value: int and (< 0 or > 255) } }])
            {
                var diagnostic = Diagnostic.Create(Rule, invocationOperation.Syntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}