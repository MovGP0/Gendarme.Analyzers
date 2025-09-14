using System.Text.RegularExpressions;

namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCorrectPrefixAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseCorrectPrefixTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseCorrectPrefixMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseCorrectPrefixDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseCorrectPrefix,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly Regex GenericParameterRegex = new Regex(@"^T[A-Z][a-zA-Z]*$", RegexOptions.Compiled);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Analyze interface and class declarations via syntax for precise spans
        context.RegisterSyntaxNodeAction(AnalyzeInterfaceDeclaration, SyntaxKind.InterfaceDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        
        // Analyze generic type parameters via syntax to ensure broad support
        context.RegisterSyntaxNodeAction(AnalyzeTypeParameterSyntax, SyntaxKind.TypeParameter);
    }

    private static void AnalyzeInterfaceDeclaration(SyntaxNodeAnalysisContext context)
    {
        var node = (InterfaceDeclarationSyntax)context.Node;
        var identifier = node.Identifier;
        var name = identifier.ValueText;

        // Interfaces should start with 'I' followed by uppercase
        if (!name.StartsWith("I") || name.Length == 1 || !char.IsUpper(name[1]))
        {
            var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), "Interface", name, "should be prefixed with 'I'");
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var node = (ClassDeclarationSyntax)context.Node;
        var identifier = node.Identifier;
        var name = identifier.ValueText;

        // Types should not start with 'C' followed by uppercase
        if (name.Length > 1 && name[0] == 'C' && char.IsUpper(name[1]))
        {
            var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), "Type", name, "should not be prefixed with 'C'");
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeTypeParameterSyntax(SyntaxNodeAnalysisContext context)
    {
        var node = (TypeParameterSyntax)context.Node;
        var identifier = node.Identifier;
        var name = identifier.ValueText;

        // Generic parameters should be a single uppercase letter or prefixed with 'T'
        var isSingleUpper = name.Length == 1 && name[0] >= 'A' && name[0] <= 'Z';
        var matchesTPrefix = GenericParameterRegex.IsMatch(name);

        if (!isSingleUpper && !matchesTPrefix)
        {
            var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), "Generic parameter", name, "should be a single uppercase letter or prefixed with 'T'");
            context.ReportDiagnostic(diagnostic);
        }
    }
}