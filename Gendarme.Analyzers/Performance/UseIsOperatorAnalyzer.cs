using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

/// <summary>
/// Suggests using the <c>is</c> operator instead of combining <c>as</c> with null checks.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseIsOperatorAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseIsOperatorTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseIsOperatorMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseIsOperatorDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseIsOperator,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeBinaryOperation, OperationKind.BinaryOperator);
    }

    private static void AnalyzeBinaryOperation(OperationAnalysisContext context)
    {
        var operation = (IBinaryOperation)context.Operation;

        if (operation.OperatorKind is not (BinaryOperatorKind.NotEquals or BinaryOperatorKind.Equals))
        {
            return;
        }

        if (!IsNull(operation.LeftOperand) && !IsNull(operation.RightOperand))
        {
            return;
        }

        if (!TryGetAsConversion(operation.LeftOperand, out var conversion) &&
            !TryGetAsConversion(operation.RightOperand, out conversion))
        {
            return;
        }

        if (!conversion.Syntax.IsKind(SyntaxKind.AsExpression))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax.ToString());
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsNull(IOperation operand) =>
        operand.ConstantValue is { HasValue: true, Value: null };

    private static bool TryGetAsConversion(IOperation operand, out IConversionOperation conversion)
    {
        conversion = null!;

        operand = UnwrapParentheses(operand);
        if (operand is IConversionOperation candidate)
        {
            conversion = candidate;
            return true;
        }

        return false;
    }

    private static IOperation UnwrapParentheses(IOperation operation)
    {
        while (operation is IParenthesizedOperation parenthesized)
        {
            operation = parenthesized.Operand;
        }

        return operation;
    }
}