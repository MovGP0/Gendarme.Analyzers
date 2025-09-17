using System.Xml;

namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProvideValidXmlStringAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ProvideValidXmlString_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ProvideValidXmlString_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ProvideValidXmlString_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ProvideValidXmlString,
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
        if (context.Node is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax memberAccess
            } invocationExpression
            || memberAccess.Name.Identifier.Text != "LoadXml"
            || invocationExpression.ArgumentList.Arguments is not { Count: > 0 } argumentList
            || argumentList[0].Expression is not LiteralExpressionSyntax xmlLiteral
            || !xmlLiteral.IsKind(SyntaxKind.StringLiteralExpression)
            || IsXmlString(xmlLiteral.Token.ValueText))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, xmlLiteral.GetLocation(), "LoadXml");
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsXmlString(string xmlString)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlString);
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }
}
