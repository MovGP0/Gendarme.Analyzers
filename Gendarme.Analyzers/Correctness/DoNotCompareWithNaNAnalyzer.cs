namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotCompareWithNaNAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotCompareWithNaN_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotCompareWithNaN_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotCompareWithNaN_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotCompareWithNaN,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        if (!IsNaN(binaryExpression.Left, context) && !IsNaN(binaryExpression.Right, context))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, binaryExpression.GetLocation()));
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        var methodSymbol = symbolInfo.Symbol as IMethodSymbol ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();

        if (methodSymbol is null || methodSymbol.Name != "Equals" || invocation.ArgumentList is null)
        {
            return;
        }

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (IsNaN(argument.Expression, context))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
                return;
            }
        }
    }

    private static bool IsNaN(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
        var semanticModel = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        var constantValue = semanticModel.GetConstantValue(expression, cancellationToken);
        if (constantValue.HasValue)
        {
            if (constantValue.Value is double doubleValue && double.IsNaN(doubleValue))
            {
                return true;
            }

            if (constantValue.Value is float floatValue && float.IsNaN(floatValue))
            {
                return true;
            }
        }

        var symbolInfo = semanticModel.GetSymbolInfo(expression, cancellationToken);
        if (IsNaNField(symbolInfo.Symbol as IFieldSymbol))
        {
            return true;
        }

        foreach (var candidate in symbolInfo.CandidateSymbols.OfType<IFieldSymbol>())
        {
            if (IsNaNField(candidate))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNaNField(IFieldSymbol? fieldSymbol)
    {
        if (fieldSymbol is null || !fieldSymbol.IsStatic || !fieldSymbol.HasConstantValue)
        {
            return false;
        }

        return fieldSymbol.ConstantValue switch
        {
            double doubleValue => double.IsNaN(doubleValue),
            float floatValue => float.IsNaN(floatValue),
            _ => false,
        };
    }
}
