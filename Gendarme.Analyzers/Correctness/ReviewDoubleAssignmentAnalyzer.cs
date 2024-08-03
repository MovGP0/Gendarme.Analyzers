namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewDoubleAssignmentAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ReviewDoubleAssignment_Title;
    private static readonly LocalizableString MessageFormat = Strings.ReviewDoubleAssignment_Message;
    private static readonly LocalizableString Description = Strings.ReviewDoubleAssignment_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewDoubleAssignment,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleAssignmentExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AssignmentExpressionSyntax assignmentExpression
            || !assignmentExpression.Left.IsEquivalentTo(assignmentExpression.Right))
        {
            return;
        }

        var location = assignmentExpression.GetLocation();
        var diagnostic = Diagnostic.Create(Rule, location, assignmentExpression.Left.ToString());
        context.ReportDiagnostic(diagnostic);
    }
}