namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FinalizersShouldBeProtectedAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.FinalizersShouldBeProtectedTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.FinalizersShouldBeProtectedMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.FinalizersShouldBeProtectedDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.FinalizersShouldBeProtected,
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
        // A finalizer is a method named '~ClassName' with MethodKind.Destructor
        var destructors = namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Destructor);

        foreach (var destructor in destructors)
        {
            // The C# compiler typically forces finalizers to be "protected" in C#, 
            // but other languages may not. Check accessibility.
            if (destructor.DeclaredAccessibility != Accessibility.Protected)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    destructor.Locations.FirstOrDefault(),
                    namedType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}