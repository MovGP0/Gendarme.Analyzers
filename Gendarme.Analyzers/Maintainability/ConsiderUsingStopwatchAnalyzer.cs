namespace Gendarme.Analyzers.Maintainability;

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
        DiagnosticSeverity.Info,
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
            if (variable.Initializer is not {} initializer
                || context.SemanticModel.GetTypeInfo(initializer.Value).Type is not {} symbol
                || symbol.ToString() != "System.DateTime"
                || initializer.Value is not InvocationExpressionSyntax invocation
                || context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol invokedMethod
                || invokedMethod.ContainingType.ToString() != "System.DateTime"
                || invokedMethod.Name != "get_Now")
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
                var diagnostic = Diagnostic.Create(Rule, initializer.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}