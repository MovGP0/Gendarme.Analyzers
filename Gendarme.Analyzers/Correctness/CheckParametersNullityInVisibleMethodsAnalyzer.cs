namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CheckParametersNullityInVisibleMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.CheckParametersNullityInVisibleMethods_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.CheckParametersNullityInVisibleMethods_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.CheckParametersNullityInVisibleMethods_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.CheckParametersNullityInVisibleMethods,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (!methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword) &&
            !methodDeclaration.Modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            return;
        }

        foreach (var parameter in methodDeclaration.ParameterList.Parameters)
        {
            if (parameter.Type is null
                || context.SemanticModel.GetTypeInfo(parameter.Type).Type is not { IsReferenceType: true }
                || HasNullCheck(methodDeclaration, parameter.Identifier.Text))
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, parameter.GetLocation(), parameter.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasNullCheck(MethodDeclarationSyntax methodDeclaration, string parameterName)
    {
        return methodDeclaration.Body?.DescendantNodes()
            .OfType<BinaryExpressionSyntax>()
            .Any(expr => expr.IsKind(SyntaxKind.EqualsExpression) &&
                         expr.Left is IdentifierNameSyntax leftIdentifier &&
                         leftIdentifier.Identifier.Text == parameterName &&
                         expr.Right.IsKind(SyntaxKind.NullLiteralExpression)) == true;
    }
}
