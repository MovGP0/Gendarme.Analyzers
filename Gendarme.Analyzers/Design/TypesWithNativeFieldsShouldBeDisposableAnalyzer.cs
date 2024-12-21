namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TypesWithNativeFieldsShouldBeDisposableAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.TypesWithNativeFieldsShouldBeDisposableTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.TypesWithNativeFieldsShouldBeDisposableMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.TypesWithNativeFieldsShouldBeDisposableDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.TypesWithNativeFieldsShouldBeDisposable,
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

        var nativeFields = namedType.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f is { IsStatic: false, IsConst: false } && 
                        (f.Type.ToDisplayString() == "System.IntPtr" ||
                         f.Type.ToDisplayString() == "System.UIntPtr" ||
                         f.Type.ToDisplayString() == "System.Runtime.InteropServices.HandleRef"))
            .ToList();

        if (nativeFields.Count == 0)
            return;

        bool implementsIDisposable = namedType.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable");

        if (!implementsIDisposable)
        {
            foreach (var field in nativeFields)
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