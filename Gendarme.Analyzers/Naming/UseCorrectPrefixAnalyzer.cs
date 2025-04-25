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

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeTypeParameter, SymbolKind.TypeParameter);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        var name = typeSymbol.Name;

        if (typeSymbol.TypeKind == TypeKind.Interface)
        {
            // Interfaces should start with 'I'
            if (!name.StartsWith("I") || name.Length == 1 || !char.IsUpper(name[1]))
            {
                var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], "Interface", name, "should be prefixed with 'I'");
                context.ReportDiagnostic(diagnostic);
            }
        }
        else
        {
            // Types should not start with 'C'
            if (name.StartsWith("C") && name.Length > 1 && char.IsUpper(name[1]))
            {
                var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], "Type", name, "should not be prefixed with 'C'");
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeTypeParameter(SymbolAnalysisContext context)
    {
        var typeParameter = (ITypeParameterSymbol)context.Symbol;
        var name = typeParameter.Name;

        // Generic parameters should be a single uppercase letter or prefixed with 'T'
        if (!(name.Length == 1 && char.IsUpper(name[0])) && !GenericParameterRegex.IsMatch(name))
        {
            var diagnostic = Diagnostic.Create(Rule, typeParameter.Locations[0], "Generic parameter", name, "should be a single uppercase letter or prefixed with 'T'");
            context.ReportDiagnostic(diagnostic);
        }
    }
}