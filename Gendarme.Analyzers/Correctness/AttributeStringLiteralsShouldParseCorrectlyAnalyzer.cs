namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AttributeStringLiteralsShouldParseCorrectlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AttributeStringLiteralsShouldParseCorrectly_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AttributeStringLiteralsShouldParseCorrectly_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AttributeStringLiteralsShouldParseCorrectly_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AttributeStringLiteralsShouldParseCorrectly,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.Attribute);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var attribute = (AttributeSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol symbol
            || attribute.ArgumentList is not { } argumentList)
        {
            return;
        }

        foreach (var argument in argumentList.Arguments)
        {
            var argumentType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
            if (argumentType?.SpecialType != SpecialType.System_String)
            {
                continue;
            }

            if (argument.Expression is LiteralExpressionSyntax stringLiteral)
            {
                var valueText = stringLiteral.Token.ValueText;

                if (IsAttributeTarget(symbol, "System.Version") && !Version.TryParse(valueText, out _))
                {
                    var diagnostic = Diagnostic.Create(Rule, argument.GetLocation(), valueText);
                    context.ReportDiagnostic(diagnostic);
                }
                else if (IsAttributeTarget(symbol, "System.Guid") && !Guid.TryParse(valueText, out _))
                {
                    var diagnostic = Diagnostic.Create(Rule, argument.GetLocation(), valueText);
                    context.ReportDiagnostic(diagnostic);
                }
                else if (IsAttributeTarget(symbol, "System.Uri") && !Uri.IsWellFormedUriString(valueText, UriKind.RelativeOrAbsolute))
                {
                    var diagnostic = Diagnostic.Create(Rule, argument.GetLocation(), valueText);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsAttributeTarget(IMethodSymbol symbol, string targetType)
    {
        return symbol.ContainingType.AllInterfaces.Any(i => i.ToString() == targetType);
    }
}
