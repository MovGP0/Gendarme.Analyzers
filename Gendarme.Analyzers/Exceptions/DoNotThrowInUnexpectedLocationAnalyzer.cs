namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotThrowInUnexpectedLocationAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotThrowInUnexpectedLocationTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotThrowInUnexpectedLocationMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotThrowInUnexpectedLocationDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotThrowInUnexpectedLocation,
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
        context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
    }

    private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
    {
        var throwStatement = (ThrowStatementSyntax)context.Node;
        
        // Check if we're in a method
        var methodDeclaration = throwStatement.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration != null)
        {
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null)
                return;
                
            AnalyzeMethodForThrow(context, throwStatement, methodSymbol.Name, methodSymbol.MethodKind);
            return;
        }
        
        // Check if we're in a destructor
        var destructorDeclaration = throwStatement.Ancestors().OfType<DestructorDeclarationSyntax>().FirstOrDefault();
        if (destructorDeclaration != null)
        {
            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(destructorDeclaration);
            if (methodSymbol == null)
                return;
                
            // Use "Finalize" as the method name for destructors
            AnalyzeMethodForThrow(context, throwStatement, "Finalize", MethodKind.Destructor);
        }
    }

    private static void AnalyzeMethodForThrow(SyntaxNodeAnalysisContext context, ThrowStatementSyntax throwStatement, string methodName, MethodKind methodKind)
    {

        // List of methods where throwing exceptions is discouraged
        var methodNames = new[]
        {
            "Equals",
            "GetHashCode",
            "ToString",
            "Finalize",
            "Dispose"
        };

        if (methodNames.Contains(methodName) || methodKind == MethodKind.Destructor)
        {
            var diagnostic = Diagnostic.Create(Rule, throwStatement.GetLocation(), methodName);
            context.ReportDiagnostic(diagnostic);
        }
    }
}