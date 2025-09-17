using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CompareWithEmptyStringEfficientlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.CompareWithEmptyStringEfficientlyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.CompareWithEmptyStringEfficientlyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.CompareWithEmptyStringEfficientlyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.CompareWithEmptyStringEfficiently,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<string> EmptyStringRepresentations = ImmutableHashSet.Create("\"\"", "string.Empty", "String.Empty");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze binary expressions
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeBinaryOperation, OperationKind.BinaryOperator);
    }

    private void AnalyzeBinaryOperation(OperationAnalysisContext context)
    {
        var operation = (IBinaryOperation)context.Operation;

        if (operation.OperatorKind != BinaryOperatorKind.Equals &&
            operation.OperatorKind != BinaryOperatorKind.NotEquals)
            return;

        if (operation.LeftOperand.Type.SpecialType != SpecialType.System_String ||
            operation.RightOperand.Type.SpecialType != SpecialType.System_String)
            return;

        if (IsEmptyString(operation.LeftOperand) || IsEmptyString(operation.RightOperand))
        {
            var diagnostic = Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax.ToString());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsEmptyString(IOperation operand)
    {
        if (operand.ConstantValue is { HasValue: true, Value: string and "" })
            return true;

        var syntax = operand.Syntax;
        if (syntax is LiteralExpressionSyntax { Token.ValueText: "" })
            return true;

        if (syntax is MemberAccessExpressionSyntax { Name.Identifier.Text: "Empty" } memberAccess &&
            (memberAccess.Expression.ToString() == "string" || memberAccess.Expression.ToString() == "String"))
            return true;

        return false;
    }
}