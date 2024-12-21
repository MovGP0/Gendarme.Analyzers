namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposableTypesShouldHaveFinalizerAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.DisposableTypesShouldHaveFinalizerTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.DisposableTypesShouldHaveFinalizerMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.DisposableTypesShouldHaveFinalizerDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DisposableTypesShouldHaveFinalizer,
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

        // Check if implements IDisposable
        var implementsIDisposable = namedType.AllInterfaces.Any(i =>
            i.ToDisplayString() == "System.IDisposable");

        if (!implementsIDisposable)
            return;

        // Check if there's a finalizer => a method with name '~ClassName'
        var hasFinalizer = namedType.GetMembers().Any(m => m is IMethodSymbol { MethodKind: MethodKind.Destructor });

        // Check for native fields: IntPtr, UIntPtr, HandleRef, etc.
        var hasNativeField = namedType.GetMembers().OfType<IFieldSymbol>().Any(f =>
        {
            var fullName = f.Type.ToDisplayString();
            return fullName == "System.IntPtr"
                   || fullName == "System.UIntPtr"
                   || fullName == "System.Runtime.InteropServices.HandleRef";
        });

        // If we have native fields, we want a finalizer
        if (hasNativeField && !hasFinalizer)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                namedType.Locations[0],
                namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}