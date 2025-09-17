namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EqualsShouldHandleNullArgAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.EqualsShouldHandleNullArg_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.EqualsShouldHandleNullArg_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.EqualsShouldHandleNullArg_Description), Strings.ResourceManager, typeof(Strings));

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

        if (methodDeclaration.Identifier.Text != "Equals" ||
            methodDeclaration.ParameterList.Parameters.Count != 1)
        {
            return;
        }

        var parameter = methodDeclaration.ParameterList.Parameters[0];
        if (parameter.Type is null)
        {
            return;
        }

        var parameterType = context.SemanticModel.GetTypeInfo(parameter.Type).Type;
        if (parameterType is not { SpecialType: SpecialType.System_Object })
        {
            return;
        }

        var returnType = context.SemanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
        if (returnType is not { SpecialType: SpecialType.System_Boolean }
            || methodDeclaration.Body is not { } body)
        {
            return;
        }

        foreach (var statement in body.Statements)
        {
            if (statement is IfStatementSyntax { Condition: BinaryExpressionSyntax binaryExpression } &&
                binaryExpression.IsKind(SyntaxKind.EqualsExpression) &&
                binaryExpression.Right.IsKind(SyntaxKind.NullLiteralExpression) &&
                binaryExpression.Left is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == parameter.Identifier.Text)
            {
                return;
            }
        }

        var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
