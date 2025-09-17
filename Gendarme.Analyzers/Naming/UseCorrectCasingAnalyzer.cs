using System.Text.RegularExpressions;

namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCorrectCasingAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseCorrectCasingTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseCorrectCasingMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseCorrectCasingDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseCorrectCasing,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly Regex PascalCaseRegex = new(@"^[A-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);
    private static readonly Regex CamelCaseRegex = new(@"^[a-z][a-zA-Z0-9]*$", RegexOptions.Compiled);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamespace, SymbolKind.Namespace);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterSymbolAction(AnalyzeParameter, SymbolKind.Parameter);
    }

    private static void AnalyzeNamespace(SymbolAnalysisContext context)
    {
        var namespaceSymbol = (INamespaceSymbol)context.Symbol;
        if (namespaceSymbol.IsGlobalNamespace)
            return;

        var name = namespaceSymbol.Name;
        if (!PascalCaseRegex.IsMatch(name))
        {
            var diagnostic = Diagnostic.Create(Rule, namespaceSymbol.Locations[0], "Namespace", name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        var name = typeSymbol.Name;

        if (!PascalCaseRegex.IsMatch(name))
        {
            var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], "Type", name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;
        if (methodSymbol.MethodKind is MethodKind.Constructor or MethodKind.Destructor)
            return;

        var name = methodSymbol.Name;

        if (!PascalCaseRegex.IsMatch(name))
        {
            var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], "Method", name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeParameter(SymbolAnalysisContext context)
    {
        var parameterSymbol = (IParameterSymbol)context.Symbol;
        var name = parameterSymbol.Name;

        if (!CamelCaseRegex.IsMatch(name))
        {
            var diagnostic = Diagnostic.Create(Rule, parameterSymbol.Locations[0], "Parameter", name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}