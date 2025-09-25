using Gendarme.Analyzers.Extensions;

namespace Gendarme.Analyzers.Interoperability;

/// <summary>
/// This rule will fire if <c>Marshal.GetLastWin32Error()</c> is called, but is not called immediately after a P/Invoke.
/// This is a problem because other methods, even managed methods, may overwrite the error code.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public void DestroyError()
/// {
///     MessageBeep(2);
///     Console.WriteLine("Beep");
///     int error = Marshal.GetLastWin32Error();
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public void GetError()
/// {
///     MessageBeep(2);
///     int error = Marshal.GetLastWin32Error();
///     Console.WriteLine("Beep");
/// }
///  
/// public void DontUseGetLastError()
/// {
///     MessageBeep(2);
///     Console.WriteLine("Beep");
/// }
/// </code>
/// </example>
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
        var invocationExpression = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(invocationExpression, context.CancellationToken).Symbol is not IMethodSymbol symbolInfo)
        {
            return;
        }

        // We match by name and containing type, not by ToString
        if (!string.Equals(symbolInfo.Name, "GetLastWin32Error", StringComparison.Ordinal))
            return;

        var containingType = symbolInfo.ContainingType;
        if (containingType is null || containingType.ToDisplayString() != "System.Runtime.InteropServices.Marshal")
            return;

        var statement = invocationExpression.Ancestors().OfType<StatementSyntax>().FirstOrDefault();
        if (statement is null)
        {
            return;
        }

        var previousStatement = statement.GetPreviousStatement();
        if (previousStatement is null)
        {
            // No previous statement -> not immediately after a P/Invoke
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocationExpression.GetLocation()));
            return;
        }

        var previousInvocation = previousStatement.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (previousInvocation is null)
        {
            // Previous statement is not an invocation -> violation
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocationExpression.GetLocation()));
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(previousInvocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            // Can't resolve symbol -> be conservative and warn
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocationExpression.GetLocation()));
            return;
        }

        var hasDllImport = methodSymbol.GetAttributes()
            .Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.DllImportAttribute");

        if (!hasDllImport)
        {
            // Previous call was not a P/Invoke -> violation
            context.ReportDiagnostic(Diagnostic.Create(Rule, invocationExpression.GetLocation()));
        }
        // else: OK (immediately after P/Invoke), do not report
    }
}
