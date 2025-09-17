namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodCanBeMadeStaticAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MethodCanBeMadeStatic_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MethodCanBeMadeStatic_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MethodCanBeMadeStatic_Description), Strings.ResourceManager, typeof(Strings));

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
