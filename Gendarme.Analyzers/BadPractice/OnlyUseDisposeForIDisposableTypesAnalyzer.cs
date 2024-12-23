namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OnlyUseDisposeForIDisposableTypesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.OnlyUseDisposeForIDisposableTypes_Title;
    private static readonly LocalizableString MessageFormat = Strings.OnlyUseDisposeForIDisposableTypes_Message;
    private static readonly LocalizableString Description = Strings.OnlyUseDisposeForIDisposableTypes_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.OnlyUseDisposeForIDisposableTypes,
        Title,
        MessageFormat,
        "Usage",
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

        // Check if the method is named 'Dispose'
        if (methodDeclaration.Identifier.Text != "Dispose")
        {
            return;
        }

        var containingType = context.SemanticModel.GetDeclaredSymbol(methodDeclaration)?.ContainingType;
        if (containingType == null)
        {
            return;
        }

        // Check if the containing type implements IDisposable
        var iDisposable = context.Compilation.GetTypeByMetadataName("System.IDisposable");
        if (iDisposable == null)
        {
            return;
        }

        if (!containingType.AllInterfaces.Contains(iDisposable))
        {
            var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }
}