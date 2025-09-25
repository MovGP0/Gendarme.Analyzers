namespace Gendarme.Analyzers.Smells;

/// <summary>
/// This rule checks for the Message Chain smell.
/// This can cause problems because it means that your code is heavily coupled to the navigation structure.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public void Method(Person person)
/// {
///     person.GetPhone ().GetAreaCode ().GetCountry ().Language.ToFrench ("Hello world");
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public void Method(Language language)
/// {
///     language.ToFrench ("Hello world");
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidMessageChainsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidMessageChainsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidMessageChainsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidMessageChainsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidMessageChains,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int MaxChainLength = 3;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze member access expressions
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        var memberAccess = (MemberAccessExpressionSyntax)context.Node;

        // Start with 1 for the current member access
        int chainLength = 1;
        var expression = memberAccess.Expression;

        while (true)
        {
            expression = Unwrap(expression);

            if (expression is MemberAccessExpressionSyntax innerMemberAccess)
            {
                chainLength++;
                expression = innerMemberAccess.Expression;
                continue;
            }

            break;
        }

        if (chainLength >= MaxChainLength)
        {
            var methodDeclaration = memberAccess.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methodDeclaration != null)
            {
                var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation(), methodDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (true)
        {
            switch (expression)
            {
                case InvocationExpressionSyntax invocation:
                    expression = invocation.Expression;
                    continue;
                case ParenthesizedExpressionSyntax paren:
                    expression = paren.Expression;
                    continue;
                case ElementAccessExpressionSyntax element:
                    expression = element.Expression;
                    continue;
            }
            return expression;
        }
    }
}