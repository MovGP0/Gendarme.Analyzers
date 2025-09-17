namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferEmptyInstanceOverNullAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.PreferEmptyInstanceOverNull_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.PreferEmptyInstanceOverNull_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.PreferEmptyInstanceOverNull_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferEmptyInstanceOverNull,
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
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var returnType = context.SemanticModel.GetTypeInfo(methodDeclaration.ReturnType).Type;

        if (returnType == null || !IsStringOrCollection(returnType))
        {
            return;
        }

        foreach (var returnStatement in methodDeclaration.DescendantNodes().OfType<ReturnStatementSyntax>())
        {
            if (returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression))
            {
                var diagnostic = Diagnostic.Create(Rule, returnStatement.GetLocation(), methodDeclaration.Identifier.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        var returnType = context.SemanticModel.GetTypeInfo(propertyDeclaration.Type).Type;

        if (returnType == null || !IsStringOrCollection(returnType))
        {
            return;
        }

        foreach (var getter in propertyDeclaration.AccessorList?.Accessors ?? Enumerable.Empty<AccessorDeclarationSyntax>())
        {
            if (getter.Body != null)
            {
                foreach (var returnStatement in getter.Body.DescendantNodes().OfType<ReturnStatementSyntax>())
                {
                    if (returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression))
                    {
                        var diagnostic = Diagnostic.Create(Rule, returnStatement.GetLocation(), propertyDeclaration.Identifier.Text);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }

    private static bool IsStringOrCollection(ITypeSymbol type)
    {
        if (type.SpecialType == SpecialType.System_String)
        {
            return true;
        }

        if (type.TypeKind == TypeKind.Array)
        {
            return true;
        }

        if (type.AllInterfaces.Any(i => i.ToString() == "System.Collections.IEnumerable"))
        {
            return true;
        }

        if (type.Name is "IEnumerable" or "ICollection" or "IList")
        {
            return true;
        }

        return false;
    }
}