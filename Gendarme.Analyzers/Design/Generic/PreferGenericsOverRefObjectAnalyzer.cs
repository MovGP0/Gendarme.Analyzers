namespace Gendarme.Analyzers.Design.Generic;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PreferGenericsOverRefObjectAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId.PreferGenericsOverRefObject,
        title: Strings.PreferGenericsOverRefObject_Title,
        messageFormat: Strings.PreferGenericsOverRefObject_Message,
        category: Category.Design,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        foreach (var parameter in methodDeclaration.ParameterList.Parameters)
        {
            if (parameter.Modifiers.Any(SyntaxKind.RefKeyword) || parameter.Modifiers.Any(SyntaxKind.OutKeyword))
            {
                if (parameter.Type is PredefinedTypeSyntax type && type.Keyword.IsKind(SyntaxKind.ObjectKeyword))
                {
                    var diagnostic = Diagnostic.Create(Rule, parameter.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}