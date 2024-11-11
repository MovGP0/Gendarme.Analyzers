namespace Gendarme.Analyzers.Design.Generic;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseGenericEventHandlerAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId.UseGenericEventHandler,
        title: Strings.UseGenericEventHandler_Title,
        messageFormat: Strings.UseGenericEventHandler_Message,
        description: Strings.UseGenericEventHandler_Description,
        category: Category.Design,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeDelegateDeclaration, SyntaxKind.DelegateDeclaration);
    }

    private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is DelegateDeclarationSyntax delegateDeclaration
            && delegateDeclaration.ParameterList.Parameters.Count == 2
            && delegateDeclaration.ParameterList.Parameters[0].Type is PredefinedTypeSyntax firstParamType
            && firstParamType.Keyword.IsKind(SyntaxKind.ObjectKeyword)
            && delegateDeclaration.ParameterList.Parameters[1].Type is {} eventArgsType
            && eventArgsType.ToString().EndsWith("EventArgs"))
        {
            var diagnostic = Diagnostic.Create(Rule, delegateDeclaration.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}