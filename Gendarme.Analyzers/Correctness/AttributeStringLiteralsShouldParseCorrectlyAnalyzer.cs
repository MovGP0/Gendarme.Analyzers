namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AttributeStringLiteralsShouldParseCorrectlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.AttributeStringLiteralsShouldParseCorrectly_Title;
    private static readonly LocalizableString MessageFormat = Strings.AttributeStringLiteralsShouldParseCorrectly_Message;
    private static readonly LocalizableString Description = Strings.AttributeStringLiteralsShouldParseCorrectly_Description;

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
        var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
        if (symbol == null)
        {
            return;
        }

        foreach (var argument in attribute.ArgumentList.Arguments)
        {
            var argumentType = context.SemanticModel.GetTypeInfo(argument.Expression).Type;
            if (argumentType.SpecialType == SpecialType.System_String)
            {
                var stringLiteral = argument.Expression as LiteralExpressionSyntax;
                if (stringLiteral != null)
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
    }

    private static bool IsAttributeTarget(IMethodSymbol symbol, string targetType)
    {
        return symbol.ContainingType.AllInterfaces.Any(i => i.ToString() == targetType);
    }
}
