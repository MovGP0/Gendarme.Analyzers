namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodCanBeMadeStaticAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.MethodCanBeMadeStatic_Title;
    private static readonly LocalizableString MessageFormat = Strings.MethodCanBeMadeStatic_Message;
    private static readonly LocalizableString Description = Strings.MethodCanBeMadeStatic_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MethodCanBeMadeStatic,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Info,
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
        if (context.Node is not MethodDeclarationSyntax methodDeclaration
            || methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword)
            || methodDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword)
            || context.SemanticModel is not {} semanticModel
            || semanticModel.GetDeclaredSymbol(methodDeclaration) is not {} methodSymbol
            || methodSymbol.IsAbstract
            || methodSymbol.IsVirtual
            || methodDeclaration.Body is not {} body
            || body.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Any(identifier => semanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodDeclaration.Identifier.Text);
        context.ReportDiagnostic(diagnostic);
    }
}
