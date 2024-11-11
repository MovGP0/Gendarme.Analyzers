namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCorrectSuffixAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseCorrectSuffixTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseCorrectSuffixMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseCorrectSuffixDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseCorrectSuffix,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableDictionary<string, string[]> SuffixRules = ImmutableDictionary.CreateRange([
        new KeyValuePair<string, string[]>("System.Attribute", ["Attribute"]),
        new KeyValuePair<string, string[]>("System.EventArgs", ["EventArgs"]),
        new KeyValuePair<string, string[]>("System.Exception", ["Exception"]),
        new KeyValuePair<string, string[]>("System.IO.Stream", ["Stream"]),
        new KeyValuePair<string, string[]>("System.Security.IPermission", ["Permission"]),
        new KeyValuePair<string, string[]>("System.Security.Policy.IMembershipCondition", ["Condition"]),
        new KeyValuePair<string, string[]>("System.Collections.IDictionary", ["Dictionary"]),
        new KeyValuePair<string, string[]>("System.Collections.Generic.IDictionary`2", ["Dictionary"]),
        new KeyValuePair<string, string[]>("System.Collections.ICollection", ["Collection"]),
        new KeyValuePair<string, string[]>("System.Collections.Generic.ICollection`1", ["Collection"]),
        new KeyValuePair<string, string[]>("System.Collections.IEnumerable", ["Collection"]),
        new KeyValuePair<string, string[]>("System.Collections.Generic.IEnumerable`1", ["Collection"])
        // Add more mappings as needed
    ]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        var name = typeSymbol.Name;

        foreach (var rule in SuffixRules)
        {
            var baseTypeOrInterface = context.Compilation.GetTypeByMetadataName(rule.Key);
            if (baseTypeOrInterface == null)
                continue;

            bool inheritsOrImplements = typeSymbol.InheritsFromOrImplements(baseTypeOrInterface);
            bool nameHasCorrectSuffix = rule.Value.Any(suffix => name.EndsWith(suffix));

            if (inheritsOrImplements && !nameHasCorrectSuffix)
            {
                var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], $"Type '{name}' should end with '{string.Join("' or '", rule.Value)}' because it inherits from or implements '{baseTypeOrInterface.Name}'");
                context.ReportDiagnostic(diagnostic);
            }
            else if (!inheritsOrImplements && nameHasCorrectSuffix)
            {
                var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], $"Type '{name}' should not end with '{string.Join("' or '", rule.Value)}' because it does not inherit from or implement '{baseTypeOrInterface.Name}'");
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}