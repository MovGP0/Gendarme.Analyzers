namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewSelfAssignmentAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.ReviewSelfAssignment_Title;
    private static readonly LocalizableString MessageFormat = Strings.ReviewSelfAssignment_Message;
    private static readonly LocalizableString Description = Strings.ReviewSelfAssignment_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewSelfAssignment,
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
        var assignmentExpression = (AssignmentExpressionSyntax)context.Node;

        if (assignmentExpression.Left.IsEquivalentTo(assignmentExpression.Right))
        {
            var location = assignmentExpression.GetLocation();
            var diagnostic = Diagnostic.Create(Rule, location, assignmentExpression.Left.ToString());
            context.ReportDiagnostic(diagnostic);
        }
    }
}