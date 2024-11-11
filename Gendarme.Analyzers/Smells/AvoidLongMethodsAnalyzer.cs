namespace Gendarme.Analyzers.Smells;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidLongMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidLongMethodsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidLongMethodsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidLongMethodsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidLongMethods,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int MaxStatements = 40;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        // Skip well-known methods
        var methodName = methodDeclaration.Identifier.Text;
        var containingType = methodDeclaration.Parent as ClassDeclarationSyntax;
        var baseTypeNames = GetBaseTypeNames(context, containingType);

        if (IsWellKnownMethod(methodName, baseTypeNames))
        {
            return;
        }

        var body = methodDeclaration.Body;
        if (body != null)
        {
            var statementCount = body.Statements.Count;

            if (statementCount > MaxStatements)
            {
                var diagnostic = Diagnostic.Create(Rule, methodDeclaration.Identifier.GetLocation(), methodName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private bool IsWellKnownMethod(string methodName, IEnumerable<string> baseTypeNames)
    {
        if (methodName == "Build" && baseTypeNames.Any(bt => bt == "Bin" || bt == "Window" || bt == "Dialog"))
            return true;

        if (methodName == "InitializeComponent" && baseTypeNames.Any(bt => bt == "Form"))
            return true;

        return false;
    }

    private IEnumerable<string> GetBaseTypeNames(SyntaxNodeAnalysisContext context, ClassDeclarationSyntax classDeclaration)
    {
        var semanticModel = context.SemanticModel;
        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
        var baseType = classSymbol.BaseType;
        var baseTypeNames = new List<string>();

        while (baseType != null)
        {
            baseTypeNames.Add(baseType.Name);
            baseType = baseType.BaseType;
        }

        return baseTypeNames;
    }
}