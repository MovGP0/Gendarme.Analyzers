[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CheckNewThreadWithoutStartAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.CheckNewThreadWithoutStart_Title;
    private static readonly LocalizableString MessageFormat = Strings.CheckNewThreadWithoutStart_Message;
    private static readonly LocalizableString Description = Strings.CheckNewThreadWithoutStart_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.CheckNewThreadWithoutStart,
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
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        // Check if the created object is a Thread
        if (context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol is not INamedTypeSymbol typeSymbol || typeSymbol.ToString() != "System.Threading.Thread")
        {
            return;
        }

        if (objectCreation.Parent is not VariableDeclaratorSyntax variableDeclarator)
        {
            return;
        }

        var variableName = variableDeclarator.Identifier.Text;
        var methodDeclaration = objectCreation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration == null)
        {
            return;
        }

        // Check if the thread is started, returned, or passed as an argument
        if (!IsThreadStartedOrUsed(methodDeclaration, variableName))
        {
            var diagnostic = Diagnostic.Create(Rule, objectCreation.GetLocation(), variableName);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsThreadStartedOrUsed(MethodDeclarationSyntax methodDeclaration, string variableName)
    {
        var threadStartedOrUsed = false;

        foreach (var descendantNode in methodDeclaration.DescendantNodes())
        {
            if (descendantNode is InvocationExpressionSyntax invocation)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifierName } memberAccess &&
                    identifierName.Identifier.Text == variableName &&
                    memberAccess.Name.Identifier.Text == "Start")
                {
                    threadStartedOrUsed = true;
                    break;
                }
            }
            else if (descendantNode is ReturnStatementSyntax { Expression: IdentifierNameSyntax returnIdentifier } &&
                     returnIdentifier.Identifier.Text == variableName)
            {
                threadStartedOrUsed = true;
                break;
            }
            else if (descendantNode is ArgumentSyntax { Expression: IdentifierNameSyntax argumentIdentifier } &&
                     argumentIdentifier.Identifier.Text == variableName)
            {
                threadStartedOrUsed = true;
                break;
            }
        }

        return threadStartedOrUsed;
    }
}
