namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidThrowingBasicExceptionsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidThrowingBasicExceptionsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidThrowingBasicExceptionsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidThrowingBasicExceptionsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidThrowingBasicExceptions,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeThrowExpression, SyntaxKind.ThrowExpression);
        context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
    }

    private static void AnalyzeThrowExpression(SyntaxNodeAnalysisContext context)
    {
        var throwExpression = (ThrowExpressionSyntax)context.Node;
        AnalyzeThrow(context, throwExpression.Expression);
    }

    private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
    {
        var throwStatement = (ThrowStatementSyntax)context.Node;
        if (throwStatement.Expression != null)
        {
            AnalyzeThrow(context, throwStatement.Expression);
        }
    }

    private static void AnalyzeThrow(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        var typeInfo = context.SemanticModel.GetTypeInfo(expression).Type as INamedTypeSymbol;
        if (typeInfo == null)
            return;

        var basicExceptions = new[]
        {
            "System.Exception",
            "System.ApplicationException",
            "System.SystemException"
        };

        if (basicExceptions.Contains(typeInfo.ToString()))
        {
            var diagnostic = Diagnostic.Create(Rule, expression.GetLocation(), typeInfo.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}