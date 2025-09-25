namespace Gendarme.Analyzers.Maintainability;

/// <summary>
/// This rule checks methods for cases where a <c>System.Diagnostics.Stopwatch</c> could be used
/// instead of using <c>System.DateTime</c> to compute the time required for an action.
/// </summary>
/// <remarks>
/// <c>Stopwatch</c> is preferred because it better expresses the intent of the code and because (on some platforms at least)
/// <c>StopWatch</c> is accurate to roughly the microsecond whereas <c>DateTime.Now</c> is only accurate to 16 milliseconds or so.
/// </remarks>
/// <example>
/// Bad example:
/// <code language="C#">
/// public TimeSpan DoLongOperation()
/// {
///     DateTime start = DateTime.Now;
///     DownloadNewOpenSuseDvdIso();
///     return DateTime.Now - start;
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public TimeSpan DoLongOperation()
/// {
///     Stopwatch watch = Stopwatch.StartNew();
///     DownloadNewOpenSuseDvdIso();
///     return watch.Elapsed;
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConsiderUsingStopwatchAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ConsiderUsingStopwatchTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ConsiderUsingStopwatchMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ConsiderUsingStopwatchDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ConsiderUsingStopwatch,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Only apply to assemblies targeting .NET Framework 2.0 or later
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeMethodSyntax, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodSyntax(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        var variables = methodDeclaration
            .DescendantNodes()
            .OfType<VariableDeclarationSyntax>()
            .SelectMany(e => e.Variables);

        foreach (var variable in variables)
        {
            if (variable.Initializer is not {} initializer)
            {
                continue;
            }

            // Ensure the initializer is a DateTime.Now or DateTime.UtcNow property access
            var typeInfo = context.SemanticModel.GetTypeInfo(initializer.Value).Type;
            if (typeInfo?.SpecialType != SpecialType.System_DateTime)
            {
                continue;
            }

            if (initializer.Value is not MemberAccessExpressionSyntax memberAccess
                || context.SemanticModel.GetSymbolInfo(memberAccess).Symbol is not IPropertySymbol prop
                || prop.ContainingType.SpecialType != SpecialType.System_DateTime
                || (prop.Name != nameof(DateTime.Now) && prop.Name != nameof(DateTime.UtcNow)))
            {
                continue;
            }

            // Look for DateTime.Now usage to measure elapsed time
            var variableName = variable.Identifier.ValueText;

            var variableUsages = methodDeclaration.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(id => id.Identifier.ValueText == variableName);

            var dateTimeSubtractions = variableUsages.SelectMany(id =>
            {
                if (id.Parent is BinaryExpressionSyntax binaryExpressions && binaryExpressions.IsKind(SyntaxKind.SubtractExpression))
                {
                    return [binaryExpressions];
                }
                return Enumerable.Empty<BinaryExpressionSyntax>();
            });

            if (dateTimeSubtractions.Any())
            {
                var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}