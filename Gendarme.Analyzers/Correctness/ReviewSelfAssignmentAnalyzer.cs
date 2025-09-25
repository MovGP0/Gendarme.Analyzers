namespace Gendarme.Analyzers.Correctness;

/// <summary>
/// This rule checks for variables or fields that are assigned to themselves.
/// This won’t change the value of the variable (or fields) but should be reviewed since it could be a typo that hides a real issue in the code.
/// </summary>
/// <example>
/// <code language="c#">
/// public class Bad {
///    private int value;
///     
///    public Bad (int value)
///    {
///        // argument is assigned to itself, this.value is unchanged
///        value = value;
///    }
/// }
/// </code>
/// <code language="c#">
/// public class Good {
///     private int value;
///      
///     public Good (int value)
///     {
///         this.value = value;
///     }
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewSelfAssignmentAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewSelfAssignment_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewSelfAssignment_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewSelfAssignment_Description), Strings.ResourceManager, typeof(Strings));

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

        var left = assignmentExpression.Left;
        var right = assignmentExpression.Right;

        // Semantic check: same symbol on both sides (variable, field, property)
        var leftSymbol = context.SemanticModel.GetSymbolInfo(left, context.CancellationToken).Symbol;
        var rightSymbol = context.SemanticModel.GetSymbolInfo(right, context.CancellationToken).Symbol;

        var isSelfAssignment = leftSymbol is not null && rightSymbol is not null &&
                               SymbolEqualityComparer.Default.Equals(leftSymbol, rightSymbol);

        // Fallback to syntax equivalence for very simple cases
        if (!isSelfAssignment && !left.IsEquivalentTo(right))
            return;

        var diagnostic = Diagnostic.Create(Rule, assignmentExpression.GetLocation(), left.ToString());
        context.ReportDiagnostic(diagnostic);
    }
}