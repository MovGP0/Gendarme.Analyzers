namespace Gendarme.Analyzers.Design.Generic;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidMethodWithUnusedGenericTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId.AvoidMethodWithUnusedGenericType,
        title: Strings.AvoidMethodWithUnusedGenericTypeAnalyzer_Title,
        messageFormat: Strings.AvoidMethodWithUnusedGenericTypeAnalyzer_Message,
        category: Category.Design,
        description: Strings.AvoidMethodWithUnusedGenericTypeAnalyzer_Description,
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
        if (!methodDeclaration.TypeParameterList?.Parameters.Any() ?? true) return;

        foreach (var typeParam in methodDeclaration.TypeParameterList.Parameters)
        {
            bool isUsedInParameters = methodDeclaration.ParameterList.Parameters
                .Any(p => p.Type?.ToString().Contains(typeParam.Identifier.Text) ?? false);

            if (!isUsedInParameters)
            {
                var diagnostic = Diagnostic.Create(Rule, typeParam.GetLocation(), typeParam.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}