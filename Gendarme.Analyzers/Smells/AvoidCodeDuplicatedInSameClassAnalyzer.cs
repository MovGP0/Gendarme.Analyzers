namespace Gendarme.Analyzers.Smells;

/// <summary>
/// Detecting code duplication accurately is complex.
/// This implementation uses a simplistic approach by comparing method bodies as strings.
/// In a production analyzer, you might use more advanced techniques like code hashing or abstract syntax tree comparison.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidCodeDuplicatedInSameClassAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidCodeDuplicatedInSameClassTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidCodeDuplicatedInSameClassMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidCodeDuplicatedInSameClassDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidCodeDuplicatedInSameClass,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze class declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        var methods = classDeclaration.Members.OfType<MethodDeclarationSyntax>().ToList();

        var methodBodies = methods.Select(m => m.Body?.ToString() ?? m.ExpressionBody?.Expression.ToString()).ToList();

        var duplicates = FindDuplicates(methodBodies);

        if (duplicates.Any())
        {
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<(int, int)> FindDuplicates(List<string> methodBodies)
    {
        var duplicates = new List<(int, int)>();

        for (int i = 0; i < methodBodies.Count; i++)
        {
            for (int j = i + 1; j < methodBodies.Count; j++)
            {
                if (methodBodies[i] != null && methodBodies[i] == methodBodies[j])
                {
                    duplicates.Add((i, j));
                }
            }
        }

        return duplicates;
    }
}