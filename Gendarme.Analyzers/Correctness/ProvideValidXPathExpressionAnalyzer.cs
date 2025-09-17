using System.Xml.XPath;

namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProvideValidXPathExpressionAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ProvideValidXPathExpression_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ProvideValidXPathExpression_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ProvideValidXPathExpression_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ProvideValidXPathExpression,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess } invocationExpression
            || (memberAccess.Name.Identifier.Text != "SelectNodes" && memberAccess.Name.Identifier.Text != "Compile")
            || invocationExpression.ArgumentList is not { Arguments: { Count: >0 } arguments }
            || arguments[0].Expression is not LiteralExpressionSyntax xpathLiteral
            || !xpathLiteral.IsKind(SyntaxKind.StringLiteralExpression)
            || IsValidXPathExpression(xpathLiteral.Token.ValueText))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, xpathLiteral.GetLocation(), memberAccess.Name.Identifier.Text);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsValidXPathExpression(string xpathExpression)
    {
        try
        {
            XPathExpression.Compile(xpathExpression);
            return true;
        }
        catch (XPathException)
        {
            return false;
        }
    }
}
