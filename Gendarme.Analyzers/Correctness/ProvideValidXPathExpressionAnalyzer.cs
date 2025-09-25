using System.Xml.XPath;

namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProvideValidXPathExpressionAnalyzer : DiagnosticAnalyzer
{
    private const string XmlNodeTypeMetadataName = "System.Xml.XmlNode";
    private const string XPathExpressionTypeMetadataName = "System.Xml.XPath.XPathExpression";

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
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax { ArgumentList.Arguments: { Count: > 0 } arguments } invocation)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (!IsTargetMethod(methodSymbol, context.Compilation))
        {
            return;
        }

        if (methodSymbol.Parameters.Length == 0 || methodSymbol.Parameters[0].Type.SpecialType != SpecialType.System_String)
        {
            return;
        }

        var xpathArgument = arguments[0].Expression;
        if (!TryGetConstantString(xpathArgument, context.SemanticModel, context.CancellationToken, out var xpathExpression))
        {
            return;
        }

        if (IsValidXPathExpression(xpathExpression))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, xpathArgument.GetLocation(), methodSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool TryGetConstantString(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out string value)
    {
        var constantValue = semanticModel.GetConstantValue(expression, cancellationToken);
        if (constantValue.HasValue && constantValue.Value is string stringValue)
        {
            value = stringValue;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool IsTargetMethod(IMethodSymbol methodSymbol, Compilation compilation)
    {
        return methodSymbol.Name switch
        {
            "Compile" => IsXPathExpressionCompile(methodSymbol, compilation),
            "SelectNodes" => IsXmlNodeSelectNodes(methodSymbol, compilation),
            _ => false,
        };
    }

    private static bool IsXPathExpressionCompile(IMethodSymbol methodSymbol, Compilation compilation)
    {
        var targetType = compilation.GetTypeByMetadataName(XPathExpressionTypeMetadataName);
        return targetType is not null && SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType, targetType);
    }

    private static bool IsXmlNodeSelectNodes(IMethodSymbol methodSymbol, Compilation compilation)
    {
        var xmlNodeType = compilation.GetTypeByMetadataName(XmlNodeTypeMetadataName);
        if (xmlNodeType is null)
        {
            return false;
        }

        for (var type = methodSymbol.ContainingType; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type, xmlNodeType))
            {
                return true;
            }
        }

        return false;
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
