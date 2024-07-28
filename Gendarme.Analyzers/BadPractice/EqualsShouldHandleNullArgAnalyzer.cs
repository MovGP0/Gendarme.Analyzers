namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EqualsShouldHandleNullArgAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.EqualsShouldHandleNullArg_Title;
    private static readonly LocalizableString MessageFormat = Strings.EqualsShouldHandleNullArg_Message;
    private static readonly LocalizableString Description = Strings.EqualsShouldHandleNullArg_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.EqualsShouldHandleNullArg,
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

        // Check if the method is named 'Equals' and has one parameter of type 'object'
        if (methodDeclaration.Identifier.Text != "Equals" ||
            methodDeclaration.ParameterList.Parameters.Count != 1 ||
            !methodDeclaration.ParameterList.Parameters[0].Type.ToString().Equals("object", StringComparison.Ordinal))
        {
            return;
        }

        // Check if the method returns bool
        var returnType = context.SemanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
        if (returnType is not { SpecialType: SpecialType.System_Boolean })
        {
            return;
        }

        // Check if the method contains a null check for the parameter
        foreach (var statement in methodDeclaration.Body.Statements)
        {
            if (statement is IfStatementSyntax { Condition: BinaryExpressionSyntax binaryExpression } &&
                binaryExpression.IsKind(SyntaxKind.EqualsExpression) &&
                binaryExpression.Right.IsKind(SyntaxKind.NullLiteralExpression) &&
                binaryExpression.Left is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == methodDeclaration.ParameterList.Parameters[0].Identifier.Text)
            {
                return;
            }
        }

        var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}