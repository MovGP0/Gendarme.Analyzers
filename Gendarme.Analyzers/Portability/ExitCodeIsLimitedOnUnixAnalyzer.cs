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
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeReturnStatement, OperationKind.Return);
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeReturnStatement(OperationAnalysisContext context)
    {
        if (context.ContainingSymbol is not IMethodSymbol { Name: "Main", ReturnType.SpecialType: SpecialType.System_Int32 })
        {
            return;
        }

        var returnOperation = (IReturnOperation)context.Operation;
        var returnedValue = returnOperation.ReturnedValue;
        if (returnedValue is { ConstantValue: { HasValue: true, Value: int value } } && (value < 0 || value > 255))
        {
            var diagnostic = Diagnostic.Create(Rule, returnOperation.Syntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeAssignment(OperationAnalysisContext context)
    {
        var assignmentOperation = (ISimpleAssignmentOperation)context.Operation;

        if (assignmentOperation.Target is not IPropertyReferenceOperation { Property: { } property })
        {
            return;
        }

        if (property.Name != "ExitCode")
        {
            return;
        }

        var containingType = property.ContainingType;
        if (containingType is null || containingType.Name != "Environment")
        {
            return;
        }

        var assignedValue = assignmentOperation.Value;
        if (assignedValue is { ConstantValue: { HasValue: true, Value: int value } } && (value < 0 || value > 255))
        {
            var diagnostic = Diagnostic.Create(Rule, assignmentOperation.Syntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocationOperation = (IInvocationOperation)context.Operation;

        if (invocationOperation.TargetMethod is not { Name: "Exit", ContainingType: { } containingType })
        {
            return;
        }

        if (containingType.Name != "Environment")
        {
            return;
        }

        var argument = invocationOperation.Arguments.FirstOrDefault();
        if (argument?.Value is { ConstantValue: { HasValue: true, Value: int value } } && (value < 0 || value > 255))
        {
            var diagnostic = Diagnostic.Create(Rule, invocationOperation.Syntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
