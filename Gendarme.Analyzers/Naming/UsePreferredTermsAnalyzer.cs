namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UsePreferredTermsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UsePreferredTermsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UsePreferredTermsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UsePreferredTermsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UsePreferredTerms,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableDictionary<string, string> PreferredTerms = ImmutableDictionary.CreateRange([
        new KeyValuePair<string, string>("Cancelled", "Canceled"),
        new KeyValuePair<string, string>("Arent", "AreNot"),
        new KeyValuePair<string, string>("Cant", "Cannot"),
        new KeyValuePair<string, string>("ComPlus", "EnterpriseServices"),
        new KeyValuePair<string, string>("Couldnt", "CouldNot"),
        new KeyValuePair<string, string>("Didnt", "DidNot"),
        new KeyValuePair<string, string>("Doesnt", "DoesNot"),
        new KeyValuePair<string, string>("Dont", "DoNot"),
        new KeyValuePair<string, string>("Hadnt", "HadNot"),
        new KeyValuePair<string, string>("Hasnt", "HasNot"),
        new KeyValuePair<string, string>("Havent", "HaveNot"),
        new KeyValuePair<string, string>("Indices", "Indexes"),
        new KeyValuePair<string, string>("Isnt", "IsNot"),
        new KeyValuePair<string, string>("LogIn", "LogOn"),
        new KeyValuePair<string, string>("LogOut", "LogOff"),
        new KeyValuePair<string, string>("Shouldnt", "ShouldNot"),
        new KeyValuePair<string, string>("SignOn", "SignIn"),
        new KeyValuePair<string, string>("SignOff", "SignOut"),
        new KeyValuePair<string, string>("Wasnt", "WasNot"),
        new KeyValuePair<string, string>("Werent", "WereNot"),
        new KeyValuePair<string, string>("Wont", "WillNot"),
        new KeyValuePair<string, string>("Wouldnt", "WouldNot"),
        new KeyValuePair<string, string>("Writeable", "Writable")
        // Add more mappings as needed
    ]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol,
            SymbolKind.NamedType,
            SymbolKind.Method,
            SymbolKind.Property,
            SymbolKind.Field,
            SymbolKind.Event,
            SymbolKind.Namespace);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;
        var name = symbol.Name;

        foreach (var term in PreferredTerms)
        {
            if (name.Contains(term.Key))
            {
                var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], name, term.Key, term.Value);
                context.ReportDiagnostic(diagnostic);
                break;
            }
        }
    }
}