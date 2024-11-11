namespace Gendarme.Analyzers.Smells;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidLargeClassesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidLargeClassesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidLargeClassesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidLargeClassesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidLargeClasses,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int MaxFieldCount = 25;

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

        var fieldDeclarations = classDeclaration.Members.OfType<FieldDeclarationSyntax>();

        var fieldCount = fieldDeclarations.SelectMany(f => f.Declaration.Variables).Count();

        if (fieldCount > MaxFieldCount)
        {
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Check for fields with common prefixes
        var fieldNames = fieldDeclarations.SelectMany(f => f.Declaration.Variables)
            .Select(v => v.Identifier.Text);

        var groupedByPrefix = fieldNames.GroupBy(name => GetPrefix(name))
            .Where(g => g.Count() > 1);

        if (groupedByPrefix.Any())
        {
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private string GetPrefix(string fieldName)
    {
        int index = 0;
        while (index < fieldName.Length && char.IsLetter(fieldName[index]))
        {
            index++;
        }
        return fieldName.Substring(0, index);
    }
}