namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CheckNewExceptionWithoutThrowingAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.CheckNewExceptionWithoutThrowing_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.CheckNewExceptionWithoutThrowing_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.CheckNewExceptionWithoutThrowing_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.CheckNewExceptionWithoutThrowing,
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

        // Check if the created object is an exception
        if (context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol is not INamedTypeSymbol typeSymbol || !IsExceptionType(typeSymbol, context))
        {
            return;
        }

        var parent = objectCreation.Parent;

        // Check if the exception object is used correctly
        if (parent is ThrowStatementSyntax ||
            parent is ReturnStatementSyntax ||
            IsPassedAsArgument(parent))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, objectCreation.GetLocation(), typeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsExceptionType(INamedTypeSymbol typeSymbol, SyntaxNodeAnalysisContext context)
    {
        var exceptionType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Exception");
        return exceptionType != null && typeSymbol.AllInterfaces.Contains(exceptionType);
    }

    /// <summary>
    /// Checks if the parent node is an argument in a method call
    /// </summary>
    private static bool IsPassedAsArgument(SyntaxNode? parent)
    {
        if (parent is not ArgumentSyntax argument)
        {
            return false;
        }

        var argumentParent = argument.Parent;
        return argumentParent is ArgumentListSyntax
               && argumentParent.Parent is InvocationExpressionSyntax;
    }
}
