namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CheckNewExceptionWithoutThrowingAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.CheckNewExceptionWithoutThrowing_Title;
    private static readonly LocalizableString MessageFormat = Strings.CheckNewExceptionWithoutThrowing_Message;
    private static readonly LocalizableString Description = Strings.CheckNewExceptionWithoutThrowing_Description;

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
        var typeSymbol = context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol as INamedTypeSymbol;

        // Check if the created object is an exception
        if (typeSymbol == null || !IsExceptionType(typeSymbol, context))
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

    private static bool IsPassedAsArgument(SyntaxNode parent)
    {
        // Check if the parent node is an argument in a method call
        if (parent is ArgumentSyntax argument)
        {
            var argumentParent = argument.Parent;
            return argumentParent is ArgumentListSyntax && argumentParent.Parent is InvocationExpressionSyntax;
        }
        return false;
    }
}
