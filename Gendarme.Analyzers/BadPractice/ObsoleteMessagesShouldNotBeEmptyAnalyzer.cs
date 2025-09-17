namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ObsoleteMessagesShouldNotBeEmptyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ObsoleteMessagesShouldNotBeEmpty_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ObsoleteMessagesShouldNotBeEmpty_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ObsoleteMessagesShouldNotBeEmpty_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ObsoleteMessagesShouldNotBeEmpty,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;

        // Check if the attribute is [Obsolete]
        if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol symbol
            || symbol.ContainingType.ToString() != "System.ObsoleteAttribute")
        {
            return;
        }

        // Check if the Obsolete attribute has an empty message
        if (attributeSyntax.ArgumentList == null || !attributeSyntax.ArgumentList.Arguments.Any())
        {
            var diagnostic = Diagnostic.Create(Rule, attributeSyntax.GetLocation(), context.ContainingSymbol?.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        var messageArgument = attributeSyntax.ArgumentList.Arguments.FirstOrDefault();
        if (messageArgument == null || messageArgument.Expression is LiteralExpressionSyntax literalExpression &&
            literalExpression.IsKind(SyntaxKind.StringLiteralExpression) &&
            string.IsNullOrWhiteSpace(literalExpression.Token.ValueText))
        {
            var diagnostic = Diagnostic.Create(Rule, attributeSyntax.GetLocation(), context.ContainingSymbol?.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}