namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseGetInterfaceToCheckAssignabilityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotUseGetInterfaceToCheckAssignability_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = "Replace 'type.GetInterface(\"{0}\") != null' with 'typeof({0}).IsAssignableFrom(type)'";
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotUseGetInterfaceToCheckAssignability_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotUseGetInterfaceToCheckAssignability,
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
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || memberAccess.Name.Identifier.Text != "GetInterface")
        {
            return;
        }

        var argumentList = invocation.ArgumentList.Arguments;
        if (argumentList.Count != 1)
        {
            return;
        }

        if (argumentList[0].Expression is not LiteralExpressionSyntax argument
            || !argument.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return;
        }

        if (invocation.Parent is not BinaryExpressionSyntax parentExpression
            || !parentExpression.OperatorToken.IsKind(SyntaxKind.ExclamationEqualsToken))
        {
            return;
        }

        if (parentExpression.Right is not LiteralExpressionSyntax right
            || !right.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), argument.Token.ValueText);
        context.ReportDiagnostic(diagnostic);
    }
}