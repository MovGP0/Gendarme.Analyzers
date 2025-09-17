using System.Text.RegularExpressions;

namespace Gendarme.Analyzers.Naming;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidNonAlphanumericIdentifierAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidNonAlphanumericIdentifierTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidNonAlphanumericIdentifierMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidNonAlphanumericIdentifierDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidNonAlphanumericIdentifier,
        Title,
        MessageFormat,
        Category.Naming,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly Regex NonAlphanumericRegex = new(
        "[^a-zA-Z0-9]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType, SymbolKind.Namespace, SymbolKind.Method, SymbolKind.Field, SymbolKind.Property, SymbolKind.Event);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var symbol = context.Symbol;
        var name = symbol.Name;

        if (symbol is INamedTypeSymbol { TypeKind: TypeKind.Interface } namedTypeSymbol)
        {
            var hasInterfaceType = namedTypeSymbol.GetAttributes()
                .Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.InterfaceTypeAttribute");
            var hasGuid = namedTypeSymbol.GetAttributes()
                .Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.GuidAttribute");
            if (hasInterfaceType && hasGuid)
            {
                return;
            }
        }

        if (!NonAlphanumericRegex.IsMatch(name))
        {
            return;
        }

        var location = symbol.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, location, name);
        context.ReportDiagnostic(diagnostic);
    }
}
