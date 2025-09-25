namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReplaceIncompleteOddnessCheckAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReplaceIncompleteOddnessCheck_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReplaceIncompleteOddnessCheck_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReplaceIncompleteOddnessCheck_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReplaceIncompleteOddnessCheck,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.EqualsExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;
        var left = RemoveParentheses(binaryExpression.Left);
        var right = RemoveParentheses(binaryExpression.Right);

        if (TryGetModuloByTwo(left, context.SemanticModel, context.CancellationToken) is not null)
        {
            if (IsComparisonTarget(right, context.SemanticModel, context.CancellationToken))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, binaryExpression.GetLocation()));
            }

            return;
        }

        if (TryGetModuloByTwo(right, context.SemanticModel, context.CancellationToken) is not null
            && IsComparisonTarget(left, context.SemanticModel, context.CancellationToken))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, binaryExpression.GetLocation()));
        }
    }

    private static BinaryExpressionSyntax? TryGetModuloByTwo(ExpressionSyntax expression, SemanticModel semanticModel, System.Threading.CancellationToken cancellationToken)
    {
        expression = RemoveParentheses(expression);

        if (expression is not BinaryExpressionSyntax moduloExpression || !moduloExpression.IsKind(SyntaxKind.ModuloExpression))
        {
            return null;
        }

        if (!IsConstantEqualTo(moduloExpression.Right, semanticModel, cancellationToken, 2, -2))
        {
            return null;
        }

        var leftType = semanticModel.GetTypeInfo(moduloExpression.Left, cancellationToken).Type;
        if (leftType is null || !IsIntegralType(leftType))
        {
            return null;
        }

        return moduloExpression;
    }

    private static bool IsComparisonTarget(ExpressionSyntax expression, SemanticModel semanticModel, System.Threading.CancellationToken cancellationToken)
        => IsConstantEqualTo(expression, semanticModel, cancellationToken, 1, -1);

    private static bool IsConstantEqualTo(ExpressionSyntax expression, SemanticModel semanticModel, System.Threading.CancellationToken cancellationToken, params long[] expectedValues)
    {
        expression = RemoveParentheses(expression);

        var constantValue = semanticModel.GetConstantValue(expression, cancellationToken);
        if (!constantValue.HasValue || constantValue.Value is null)
        {
            return false;
        }

        if (!TryConvertToLong(constantValue.Value, out var value))
        {
            return false;
        }

        foreach (var expected in expectedValues)
        {
            if (value == expected)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryConvertToLong(object value, out long result)
    {
        switch (value)
        {
            case sbyte sb:
                result = sb;
                return true;
            case byte b:
                result = b;
                return true;
            case short s:
                result = s;
                return true;
            case ushort us:
                result = us;
                return true;
            case int i:
                result = i;
                return true;
            case uint ui when ui <= int.MaxValue:
                result = ui;
                return true;
            case long l:
                result = l;
                return true;
            case ulong ul when ul <= (ulong)long.MaxValue:
                result = (long)ul;
                return true;
            default:
                result = 0;
                return false;
        }
    }

    private static bool IsIntegralType(ITypeSymbol type)
    {
        return type.SpecialType is SpecialType.System_SByte
            or SpecialType.System_Byte
            or SpecialType.System_Int16
            or SpecialType.System_UInt16
            or SpecialType.System_Int32
            or SpecialType.System_UInt32
            or SpecialType.System_Int64
            or SpecialType.System_UInt64;
    }

    private static ExpressionSyntax RemoveParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }
}
