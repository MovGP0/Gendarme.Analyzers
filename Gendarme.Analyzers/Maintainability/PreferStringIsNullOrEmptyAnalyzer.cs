namespace Gendarme.Analyzers.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferStringIsNullOrEmptyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.PreferStringIsNullOrEmptyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.PreferStringIsNullOrEmptyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.PreferStringIsNullOrEmptyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferStringIsNullOrEmpty,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Only apply to assemblies targeting .NET Framework 2.0 or later
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeBinaryExpression, SyntaxKind.LogicalOrExpression, SyntaxKind.LogicalAndExpression);
    }

    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;

        if (binaryExpression.Left is not BinaryExpressionSyntax leftCondition || binaryExpression.Right is not BinaryExpressionSyntax rightCondition)
            return;

        // Check for (str == null) || (str.Length == 0)
        if (IsNullCheck(leftCondition, context, out var variableName) && IsLengthCheck(rightCondition, context, variableName))
        {
            var diagnostic = Diagnostic.Create(Rule, binaryExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
        // Check for (str != null) && (str.Length > 0)
        else if (IsNotNullCheck(leftCondition, context, out variableName) && IsLengthGreaterThanZeroCheck(rightCondition, context, variableName))
        {
            var diagnostic = Diagnostic.Create(Rule, binaryExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNullCheck(BinaryExpressionSyntax expression, SyntaxNodeAnalysisContext context, out string variableName)
    {
        variableName = string.Empty;

        if (!expression.IsKind(SyntaxKind.EqualsExpression))
            return false;

        if (expression.Left is IdentifierNameSyntax leftId && expression.Right.IsKind(SyntaxKind.NullLiteralExpression))
        {
            variableName = leftId.Identifier.ValueText;
            return IsStringType(leftId, context);
        }

        if (expression.Right is IdentifierNameSyntax rightId && expression.Left.IsKind(SyntaxKind.NullLiteralExpression))
        {
            variableName = rightId.Identifier.ValueText;
            return IsStringType(rightId, context);
        }

        return false;
    }

    private static bool IsNotNullCheck(BinaryExpressionSyntax expression, SyntaxNodeAnalysisContext context, out string variableName)
    {
        variableName = string.Empty;
        if (!expression.IsKind(SyntaxKind.NotEqualsExpression))
            return false;

        if (expression.Left is IdentifierNameSyntax leftId && expression.Right.IsKind(SyntaxKind.NullLiteralExpression))
        {
            variableName = leftId.Identifier.ValueText;
            return IsStringType(leftId, context);
        }

        if (expression.Right is IdentifierNameSyntax rightId && expression.Left.IsKind(SyntaxKind.NullLiteralExpression))
        {
            variableName = rightId.Identifier.ValueText;
            return IsStringType(rightId, context);
        }

        return false;
    }

    private static bool IsLengthCheck(BinaryExpressionSyntax expression, SyntaxNodeAnalysisContext context, string variableName)
    {
        if (!expression.IsKind(SyntaxKind.EqualsExpression))
        {
            return false;
        }

        if (expression is not { Left: MemberAccessExpressionSyntax memberAccess, Right: LiteralExpressionSyntax literal })
        {
            return false;
        }

        if (memberAccess.Expression is IdentifierNameSyntax id && id.Identifier.ValueText == variableName && memberAccess.Name.Identifier.ValueText == "Length")
        {
            return literal.Token.ValueText == "0";
        }

        return false;
    }

    private static bool IsLengthGreaterThanZeroCheck(BinaryExpressionSyntax expression, SyntaxNodeAnalysisContext context, string variableName)
    {
        if (!expression.IsKind(SyntaxKind.GreaterThanExpression))
        {
            return false;
        }

        if (expression is not { Left: MemberAccessExpressionSyntax memberAccess, Right: LiteralExpressionSyntax literal })
        {
            return false;
        }

        if (memberAccess.Expression is IdentifierNameSyntax id && id.Identifier.ValueText == variableName && memberAccess.Name.Identifier.ValueText == "Length")
        {
            return literal.Token.ValueText == "0";
        }

        return false;
    }

    private static bool IsStringType(IdentifierNameSyntax identifier, SyntaxNodeAnalysisContext context)
    {
        var symbol = context.SemanticModel.GetSymbolInfo(identifier).Symbol;
        if (symbol == null)
            return false;

        var type = (symbol as ILocalSymbol)?.Type ?? (symbol as IParameterSymbol)?.Type;
        return type?.SpecialType == SpecialType.System_String;
    }
}