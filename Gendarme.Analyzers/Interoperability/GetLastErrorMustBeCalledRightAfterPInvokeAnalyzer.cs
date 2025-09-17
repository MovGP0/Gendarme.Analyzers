using Gendarme.Analyzers.Extensions;

namespace Gendarme.Analyzers.Interoperability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GetLastErrorMustBeCalledRightAfterPInvokeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.GetLastErrorMustBeCalledRightAfterPInvokeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.GetLastErrorMustBeCalledRightAfterPInvokeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.GetLastErrorMustBeCalledRightAfterPInvokeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.GetLastErrorMustBeCalledRightAfterPInvoke,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const string GetLastWin32ErrorMethodName = "System.Runtime.InteropServices.Marshal.GetLastWin32Error";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeGetLastWin32ErrorCall, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeGetLastWin32ErrorCall(SyntaxNodeAnalysisContext context)
    {
        if (context.SemanticModel is null)
        {
            return;
        }

        var invocationExpression = (InvocationExpressionSyntax)context.Node;
        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken).Symbol as IMethodSymbol;
        if (symbolInfo is null || !string.Equals(symbolInfo.ToString(), GetLastWin32ErrorMethodName, StringComparison.Ordinal))
        {
            return;
        }

        var statement = invocationExpression.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
        if (statement is null)
        {
            return;
        }

        var previousStatement = statement.GetPreviousStatement();
        if (previousStatement is null)
        {
            return;
        }

        var invocation = previousStatement.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocation is null)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        var hasDllImport = methodSymbol.GetAttributes()
            .Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.DllImportAttribute");

        if (hasDllImport)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
