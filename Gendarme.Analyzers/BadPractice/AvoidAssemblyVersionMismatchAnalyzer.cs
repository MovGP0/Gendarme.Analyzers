namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidAssemblyVersionMismatchAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.AssemblyVersionMismatch_Title;
    private static readonly LocalizableString MessageFormat = Strings.AssemblyVersionMismatch_Message;
    private static readonly LocalizableString Description = Strings.AssemblyVersionMismatch_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AssemblyVersionMismatch,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        helpLinkUri: Strings.AssemblyVersionMismatch_HelpLink,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        var attributes = namedTypeSymbol.ContainingAssembly.GetAttributes();

        var assemblyVersionAttributes = attributes
            .Where(attr => attr.AttributeClass?.Name
                is "AssemblyVersionAttribute"
                or "AssemblyFileVersionAttribute"
                or "AssemblyInformationalVersionAttribute");

        var versions = assemblyVersionAttributes
            .Select(attr => attr.ConstructorArguments[0].Value?.ToString())
            .Distinct()
            .ToImmutableArray();

        if (versions.Length <= 1) return;

        var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], versions.First());
        context.ReportDiagnostic(diagnostic);
    }
}