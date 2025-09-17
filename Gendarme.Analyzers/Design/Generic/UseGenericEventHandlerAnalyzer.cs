namespace Gendarme.Analyzers.Design.Generic;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseGenericEventHandlerAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseGenericEventHandler_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseGenericEventHandler_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseGenericEventHandler_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseGenericEventHandler,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeDelegateDeclaration, SyntaxKind.DelegateDeclaration);
    }

    private static void AnalyzeDelegateDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is DelegateDeclarationSyntax { ParameterList.Parameters.Count: 2 } delegateDeclaration
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