namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProvideCorrectRegexPatternAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ProvideCorrectRegexPattern_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ProvideCorrectRegexPattern_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ProvideCorrectRegexPattern_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ProvideCorrectRegexPattern,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax objectCreationExpression
            || context.SemanticModel.GetTypeInfo(objectCreationExpression).Type is not INamedTypeSymbol { Name: "Regex" } typeSymbol
            || typeSymbol.ContainingNamespace.ToString() != "System.Text.RegularExpressions"
            || objectCreationExpression.ArgumentList is not { Arguments: { Count: > 0 } arguments }
            || arguments[0].Expression is not LiteralExpressionSyntax regexLiteral
            || !regexLiteral.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return;
        }

        var regexPattern = regexLiteral.Token.ValueText;
        try
        {
            _ = new System.Text.RegularExpressions.Regex(regexPattern);
        }
        catch (ArgumentException)
        {
            var diagnostic = Diagnostic.Create(Rule, regexLiteral.GetLocation(), "Regex");
            context.ReportDiagnostic(diagnostic);
        }
    }
}
