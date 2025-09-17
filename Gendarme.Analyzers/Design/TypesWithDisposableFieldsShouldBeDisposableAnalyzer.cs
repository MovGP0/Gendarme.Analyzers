namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypesWithDisposableFieldsShouldBeDisposableAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.TypesWithDisposableFieldsShouldBeDisposableTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.TypesWithDisposableFieldsShouldBeDisposableMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.TypesWithDisposableFieldsShouldBeDisposableDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.TypesWithDisposableFieldsShouldBeDisposable,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        // Collect all fields that are disposable
        var disposableFields = namedType.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f =>
            {
                // Skip const or static
                if (f.IsConst || f.IsStatic) return false;
                return f.Type.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable");
            })
            .ToList();

        if (disposableFields.Count == 0)
            return;

        // If the containing type doesn't implement IDisposable -> diagnostic
        bool implementsIDisposable = namedType.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable");

        if (!implementsIDisposable)
        {
            foreach (var field in disposableFields)
            {
                var diag = Diagnostic.Create(
                    Rule,
                    field.Locations.FirstOrDefault(),
                    namedType.Name,
                    field.Name);
                context.ReportDiagnostic(diag);
            }
        }
    }
}