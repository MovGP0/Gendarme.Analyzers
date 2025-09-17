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

    private static readonly string GetLastWin32ErrorMethodName = "System.Runtime.InteropServices.Marshal.GetLastWin32Error";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeGetLastWin32ErrorCall, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeGetLastWin32ErrorCall(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol symbol || symbol.ToString() != GetLastWin32ErrorMethodName)
            return;

        // Find the previous statement
        var statement = invocationExpression.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
        if (statement == null)
            return;

        var previousStatement = statement.GetPreviousStatement();
        if (previousStatement == null)
            return;

        // Check if the previous statement contains a P/Invoke call
        var invocation = previousStatement.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocation == null)
            return;

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            return;

        // Identify P/Invoke by checking for the DllImportAttribute
        var hasDllImport = methodSymbol.GetAttributes().Any(attr => attr.AttributeClass.ToString() == "System.Runtime.InteropServices.DllImportAttribute");
        if (!hasDllImport)
        {
            var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}