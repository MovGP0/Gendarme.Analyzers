namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidCallingProblematicMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.AvoidCallingProblematicMethods_Title;
    private static readonly LocalizableString MessageFormat = Strings.AvoidCallingProblematicMethods_Message;
    private static readonly LocalizableString Description = Strings.AvoidCallingProblematicMethods_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidCallingProblematicMethods,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    /// <summary>
    /// List of problematic methods
    /// </summary>
    private static readonly string[] ProblematicMethods =
    [
        "System.GC.Collect",
        "System.Threading.Thread.Suspend",
        "System.Threading.Thread.Resume",
        "System.Runtime.InteropServices.SafeHandle.DangerousGetHandle",
        "System.Reflection.Assembly.LoadFrom",
        "System.Reflection.Assembly.LoadFile",
        "System.Reflection.Assembly.LoadWithPartialName",
        "System.Type.InvokeMember"
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax)context.Node;

        var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

        if (symbol == null) return;

        var fullMethodName = $"{symbol.ContainingType.ToDisplayString()}.{symbol.Name}";

        // Additional check for Type.InvokeMember with BindingFlags.NonPublic
        if (fullMethodName == "System.Type.InvokeMember")
        {
            var argumentList = invocation.ArgumentList.Arguments;
            if (argumentList.Count >= 2 &&
                context.SemanticModel.GetConstantValue(argumentList[1].Expression).Value is int flags &&
                (flags & (int)System.Reflection.BindingFlags.NonPublic) != 0)
            {
                var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), symbol.Name);
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }

        if (ProblematicMethods.Contains(fullMethodName))
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.GetLocation(), symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}