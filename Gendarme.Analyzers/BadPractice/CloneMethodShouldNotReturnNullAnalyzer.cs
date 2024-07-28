namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CloneMethodShouldNotReturnNullAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.CloneMethodShouldNotReturnNull_Title;
    private static readonly LocalizableString MessageFormat = Strings.CloneMethodShouldNotReturnNull_Message;
    private static readonly LocalizableString Description = Strings.CloneMethodShouldNotReturnNull_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.CloneMethodShouldNotReturnNull,
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
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        // Check if the method is named 'Clone' and has no parameters
        if (methodDeclaration.Identifier.Text != "Clone" || methodDeclaration.ParameterList.Parameters.Count != 0)
        {
            return;
        }

        var returnType = context.SemanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
        if (returnType == null || returnType.SpecialType != SpecialType.System_Object)
        {
            return;
        }

        var containingType = context.SemanticModel.GetDeclaredSymbol(methodDeclaration)?.ContainingType;
        if (containingType == null || !containingType.AllInterfaces.Any(i => i.ToString() == "System.ICloneable"))
        {
            return;
        }

        // Analyze the method body for return statements
        foreach (var descendantNode in methodDeclaration.DescendantNodes())
        {
            if (descendantNode is ReturnStatementSyntax returnStatement)
            {
                var returnExpression = returnStatement.Expression;
                var constantValue = context.SemanticModel.GetConstantValue(returnExpression);

                if (constantValue.HasValue && constantValue.Value == null)
                {
                    var diagnostic = Diagnostic.Create(Rule, returnExpression.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
