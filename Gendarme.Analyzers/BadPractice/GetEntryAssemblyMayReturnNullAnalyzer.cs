namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GetEntryAssemblyMayReturnNullAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.GetEntryAssemblyMayReturnNull_Title;
    private static readonly LocalizableString MessageFormat = Strings.GetEntryAssemblyMayReturnNull_Message;
    private static readonly LocalizableString Description = Strings.GetEntryAssemblyMayReturnNull_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.GetEntryAssemblyMayReturnNull,
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
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Name.Identifier.Text != "GetEntryAssembly")
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(memberAccess).Symbol is not IMethodSymbol symbol
            || symbol.ContainingType.ToString() != "System.Reflection.Assembly")
        {
            return;
        }

        // Ensure the project is a library (not an executable)
        var compilation = context.SemanticModel.Compilation;
        if (compilation.Options.OutputKind
            is OutputKind.DynamicallyLinkedLibrary
            or OutputKind.NetModule
            or OutputKind.WindowsRuntimeMetadata)
        {
            var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}