[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CheckNewThreadWithoutStartAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.CheckNewThreadWithoutStart_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.CheckNewThreadWithoutStart_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.CheckNewThreadWithoutStart_Description), Strings.ResourceManager, typeof(Strings));

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

        if (objectCreation.Parent is not EqualsValueClauseSyntax equalsValueClause
            || equalsValueClause.Parent is not VariableDeclaratorSyntax variableDeclarator)
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
        foreach (var invocation in methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocation.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifierName } memberAccess &&
                identifierName.Identifier.Text == variableName &&
                memberAccess.Name.Identifier.Text == "Start")
            {
                return true;
            }
        }

        return false;
    }
}
