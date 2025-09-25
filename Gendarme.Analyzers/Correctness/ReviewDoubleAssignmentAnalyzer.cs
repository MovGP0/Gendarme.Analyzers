namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewDoubleAssignmentAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewDoubleAssignment_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewDoubleAssignment_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewDoubleAssignment_Description), Strings.ResourceManager, typeof(Strings));

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
        if (context.Node is not AssignmentExpressionSyntax assignmentExpression)
            return;

        var left = assignmentExpression.Left;
        var right = assignmentExpression.Right;

        // Prefer semantic comparison to handle cases like this.x = this.x, obj.Field = obj.Field, etc.
        var leftSymbol = context.SemanticModel.GetSymbolInfo(left).Symbol;
        var rightSymbol = context.SemanticModel.GetSymbolInfo(right).Symbol;

        var sameSymbol = leftSymbol is not null && rightSymbol is not null &&
                         SymbolEqualityComparer.Default.Equals(leftSymbol, rightSymbol);

        // As a fallback, use syntax equivalence (covers simple cases when symbol info is not available)
        if (!sameSymbol && !left.IsEquivalentTo(right))
            return;

        var location = assignmentExpression.GetLocation();
        var diagnostic = Diagnostic.Create(Rule, location, left.ToString());
        context.ReportDiagnostic(diagnostic);
    }
}