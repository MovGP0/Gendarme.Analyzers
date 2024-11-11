namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseSuppressFinalizeOnIDisposableTypeWithFinalizerTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseSuppressFinalizeOnIDisposableTypeWithFinalizerMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseSuppressFinalizeOnIDisposableTypeWithFinalizerDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseSuppressFinalizeOnIDisposableTypeWithFinalizer,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private void AnalyzeType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        // Check if type implements IDisposable
        if (!typeSymbol.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable"))
            return;

        // Check if type has a finalizer
        var hasFinalizer = typeSymbol.GetMembers().OfType<IMethodSymbol>()
            .Any(m => m.MethodKind == MethodKind.Destructor);

        if (!hasFinalizer)
            return;

        // Check if Dispose method calls GC.SuppressFinalize(this)
        var disposeMethod = typeSymbol.GetMembers("Dispose").OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Parameters.Length == 0);

        if (disposeMethod == null)
            return;

        var disposeSyntaxRef = disposeMethod.DeclaringSyntaxReferences.FirstOrDefault();
        if (disposeSyntaxRef == null)
            return;

        var disposeSyntax = disposeSyntaxRef.GetSyntax(context.CancellationToken);
        var semanticModel = context.Compilation.GetSemanticModel(disposeSyntax.SyntaxTree);

        var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(disposeSyntax);

        var callsSuppressFinalize = dataFlowAnalysis.ReadInside.OfType<IMethodSymbol>()
            .Any(m => m.Name == "SuppressFinalize" && m.ContainingType.ToDisplayString() == "System.GC");

        if (!callsSuppressFinalize)
        {
            var diagnostic = Diagnostic.Create(Rule, disposeMethod.Locations[0], typeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}