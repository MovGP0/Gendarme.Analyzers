namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseValueInPropertySetterAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.UseValueInPropertySetter_Title;
    private static readonly LocalizableString MessageFormat = Strings.UseValueInPropertySetter_Message;
    private static readonly LocalizableString Description = Strings.UseValueInPropertySetter_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseValueInPropertySetter,
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
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        var setter = propertyDeclaration.AccessorList?.Accessors
            .FirstOrDefault(a => a.Kind() == SyntaxKind.SetAccessorDeclaration);

        if (setter?.Body == null
            || setter.Body.Statements
                .OfType<ExpressionStatementSyntax>()
                .Any(stmt => stmt.Expression is AssignmentExpressionSyntax
                {
                    Right: IdentifierNameSyntax { Identifier.Text: "value" }
                }))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, setter.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}