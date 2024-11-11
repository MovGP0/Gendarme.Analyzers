namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidArgumentExceptionDefaultConstructorAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidArgumentExceptionDefaultConstructorTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidArgumentExceptionDefaultConstructorMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidArgumentExceptionDefaultConstructorDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidArgumentExceptionDefaultConstructor,
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
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        var typeSymbol = context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol as INamedTypeSymbol;
        if (typeSymbol == null)
            return;

        var exceptionTypeNames = new[]
        {
            "System.ArgumentException",
            "System.ArgumentNullException",
            "System.ArgumentOutOfRangeException",
            "System.DuplicateWaitObjectException"
        };

        if (!exceptionTypeNames.Contains(typeSymbol.ToString()))
            return;

        var constructorSymbol = context.SemanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;
        if (constructorSymbol == null)
            return;

        if (constructorSymbol.Parameters.Length == 0)
        {
            var diagnostic = Diagnostic.Create(Rule, objectCreation.GetLocation(), typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}