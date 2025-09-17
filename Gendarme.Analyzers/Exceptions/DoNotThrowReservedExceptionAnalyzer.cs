namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotThrowReservedExceptionAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotThrowReservedExceptionTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotThrowReservedExceptionMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotThrowReservedExceptionDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotThrowReservedException,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
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
        if (context.SemanticModel.GetTypeInfo(expression).Type is not INamedTypeSymbol typeInfo)
            return;

        var reservedExceptions = new[]
        {
            "System.ExecutionEngineException",
            "System.IndexOutOfRangeException",
            "System.NullReferenceException",
            "System.OutOfMemoryException"
        };

        if (reservedExceptions.Contains(typeInfo.ToString()))
        {
            var diagnostic = Diagnostic.Create(Rule, expression.GetLocation(), typeInfo.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}