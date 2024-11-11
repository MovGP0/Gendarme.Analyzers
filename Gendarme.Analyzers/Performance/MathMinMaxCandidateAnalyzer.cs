using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MathMinMaxCandidateAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MathMinMaxCandidateTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MathMinMaxCandidateMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MathMinMaxCandidateDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MathMinMaxCandidate,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze conditional expressions
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeConditionalOperation, OperationKind.Conditional);
    }

    private void AnalyzeConditionalOperation(OperationAnalysisContext context)
    {
        var conditional = (IConditionalOperation)context.Operation;

        if (conditional.Condition is IBinaryOperation comparison)
        {
            if (IsMinMaxPattern(comparison, conditional))
            {
                string methodName = comparison.OperatorKind == BinaryOperatorKind.LessThan ? "Min" : "Max";
                var diagnostic = Diagnostic.Create(Rule, conditional.Syntax.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private bool IsMinMaxPattern(IBinaryOperation comparison, IConditionalOperation conditional)
    {
        // Check for patterns like (a > b) ? a : b
        var leftOperand = comparison.LeftOperand;
        var rightOperand = comparison.RightOperand;

        var trueValue = conditional.WhenTrue;
        var falseValue = conditional.WhenFalse;

        if (leftOperand == null || rightOperand == null || trueValue == null || falseValue == null)
            return false;

        if (comparison.OperatorKind == BinaryOperatorKind.GreaterThan &&
            trueValue.Syntax.ToString() == leftOperand.Syntax.ToString() &&
            falseValue.Syntax.ToString() == rightOperand.Syntax.ToString())
        {
            return true;
        }

        if (comparison.OperatorKind == BinaryOperatorKind.LessThan &&
            trueValue.Syntax.ToString() == leftOperand.Syntax.ToString() &&
            falseValue.Syntax.ToString() == rightOperand.Syntax.ToString())
        {
            return true;
        }

        return false;
    }
}