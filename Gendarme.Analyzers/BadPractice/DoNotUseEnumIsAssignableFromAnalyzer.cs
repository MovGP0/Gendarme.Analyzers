namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseEnumIsAssignableFromAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotUseEnumIsAssignableFrom_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotUseEnumIsAssignableFrom_Message;
    private static readonly LocalizableString Description = Strings.DoNotUseEnumIsAssignableFrom_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotUseEnumIsAssignableFrom,
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

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess || memberAccess.Name.Identifier.Text != "IsAssignableFrom")
        {
            return;
        }

        if (memberAccess.Expression is not MemberAccessExpressionSyntax memberAccessExpression)
        {
            return;
        }

        if (memberAccessExpression.Expression is TypeOfExpressionSyntax typeOfExpression &&
            typeOfExpression.Type.ToString() == "Enum")
        {
            var argumentList = invocation.ArgumentList.Arguments;
            if (argumentList.Count == 1)
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), argumentList[0].Expression.ToString());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}