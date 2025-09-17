namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ToStringShouldNotReturnNullAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ToStringShouldNotReturnNull_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ToStringShouldNotReturnNull_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ToStringShouldNotReturnNull_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ToStringShouldNotReturnNull,
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

        // Check if the method is named 'ToString' and has no parameters
        if (methodDeclaration.Identifier.Text != "ToString" || methodDeclaration.ParameterList.Parameters.Count != 0)
        {
            return;
        }

        // Check if the method returns a string
        var returnType = context.SemanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;
        if (returnType is not { SpecialType: SpecialType.System_String })
        {
            return;
        }

        // Check if the method overrides an inherited method
        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol { IsOverride: true })
        {
            return;
        }

        // Analyze the method body for return statements
        foreach (var returnStatement in methodDeclaration.DescendantNodes().OfType<ReturnStatementSyntax>())
        {
            if (returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression))
            {
                var diagnostic = Diagnostic.Create(Rule, returnStatement.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}