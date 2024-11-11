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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        var containingMethod = returnOperation.SemanticModel.GetEnclosingSymbol(returnOperation.Syntax.SpanStart) as IMethodSymbol;

        if (containingMethod != null && containingMethod.Name == "Main" && containingMethod.ReturnType.SpecialType == SpecialType.System_Int32)
        {
            // Check if return value is a constant out of range 0-255
            var returnedValue = returnOperation.ReturnedValue;
            if (returnedValue != null)
            {
                var constantValue = returnedValue.ConstantValue;
                if (constantValue.HasValue && constantValue.Value is int intValue)
                {
                    if (intValue < 0 || intValue > 255)
                    {
                        var diagnostic = Diagnostic.Create(Rule, returnOperation.Syntax.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
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
                if (assignedValue != null)
                {
                    var constantValue = assignedValue.ConstantValue;
                    if (constantValue.HasValue && constantValue.Value is int intValue)
                    {
                        if (intValue < 0 || intValue > 255)
                        {
                            var diagnostic = Diagnostic.Create(Rule, assignmentOperation.Syntax.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocationOperation = (IInvocationOperation)context.Operation;

        if (invocationOperation.TargetMethod.Name == "Exit" && invocationOperation.TargetMethod.ContainingType.Name == "Environment")
        {
            if (invocationOperation.Arguments.Length == 1)
            {
                var argument = invocationOperation.Arguments[0].Value;
                if (argument != null)
                {
                    var constantValue = argument.ConstantValue;
                    if (constantValue.HasValue && constantValue.Value is int intValue)
                    {
                        if (intValue < 0 || intValue > 255)
                        {
                            var diagnostic = Diagnostic.Create(Rule, invocationOperation.Syntax.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}