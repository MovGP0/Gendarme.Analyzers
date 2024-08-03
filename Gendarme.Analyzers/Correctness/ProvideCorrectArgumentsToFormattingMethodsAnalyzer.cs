namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProvideCorrectArgumentsToFormattingMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ProvideCorrectArgumentsToFormattingMethods_Title;
    private static readonly LocalizableString MessageFormat = Strings.ProvideCorrectArgumentsToFormattingMethods_Message;
    private static readonly LocalizableString Description = Strings.ProvideCorrectArgumentsToFormattingMethods_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ProvideCorrectArgumentsToFormattingMethods,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccess ||
            memberAccess.Name.Identifier.Text != "Format" ||
            memberAccess.Expression is not IdentifierNameSyntax identifier ||
            identifier.Identifier.Text != "String")
        {
            return;
        }

        var argumentList = invocationExpression.ArgumentList.Arguments;
        if (argumentList.Count <= 0 ||
            argumentList[0].Expression is not LiteralExpressionSyntax formatStringLiteral ||
            !formatStringLiteral.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return;
        }

        var formatString = formatStringLiteral.Token.ValueText;
        var expectedArgumentCount = CountFormatPlaceholders(formatString);
        var providedArgumentCount = argumentList.Count - 1;

        if (expectedArgumentCount == providedArgumentCount)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(), "String.Format");
        context.ReportDiagnostic(diagnostic);
    }

    private static int CountFormatPlaceholders(string formatString)
    {
        var count = 0;
        for (int i = 0; i < formatString.Length - 1; i++)
        {
            if (formatString[i] == '{' && char.IsDigit(formatString[i + 1]))
            {
                count++;
            }
        }
        return count;
    }
}
