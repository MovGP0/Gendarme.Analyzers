namespace Gendarme.Analyzers.Smells;

/// <summary>
/// This rule allows developers to measure the classes size.
/// When a class is trying to do a lot of work, then you probably have the Large Class smell.
/// This rule will fire if a type contains too many fields (over 25 by default) or has fields with common prefixes.
/// If the rule does fire then the type should be reviewed to see if new classes should be extracted from it.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public class MyClass {
///     int x, x1, x2, x3;
///     string s, s1, s2, s3;
///     DateTime bar, bar1, bar2;
///     string[] strings, strings1;
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public class MyClass {
///     int x;
///     string s;
///     DateTime bar;
/// }
/// </code>
/// </example>
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
    private const int MinPrefixGroupSize = 6; // require at least 6 fields sharing a prefix to avoid false positives

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

        var groupedByPrefix = fieldNames
            .Select(GetPrefix)
            .Where(prefix => !string.IsNullOrEmpty(prefix))
            .GroupBy(prefix => prefix)
            .Where(g => g.Count() >= MinPrefixGroupSize);

        if (groupedByPrefix.Any())
        {
            var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private string GetPrefix(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
            return string.Empty;

        // Skip leading non-letter characters (e.g., '_' or 'm_')
        var i = 0;
        while (i < fieldName.Length && !char.IsLetter(fieldName[i]))
        {
            i++;
        }

        var start = i;
        while (i < fieldName.Length && char.IsLetter(fieldName[i]))
        {
            i++;
        }

        return i > start ? fieldName.Substring(start, i - start) : string.Empty;
    }
}