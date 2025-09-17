namespace Gendarme.Analyzers.Design.Generic;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidMethodWithUnusedGenericTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidMethodWithUnusedGenericTypeAnalyzer_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidMethodWithUnusedGenericTypeAnalyzer_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidMethodWithUnusedGenericTypeAnalyzer_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidMethodWithUnusedGenericType,
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
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        if (!methodDeclaration.TypeParameterList?.Parameters.Any() ?? true) return;

        foreach (var typeParam in methodDeclaration.TypeParameterList.Parameters)
        {
            var isUsedInParameters = methodDeclaration.ParameterList.Parameters
                .Any(p => p.Type?.ToString().Contains(typeParam.Identifier.Text) ?? false);

            if (!isUsedInParameters)
            {
                var diagnostic = Diagnostic.Create(Rule, typeParam.GetLocation(), typeParam.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}