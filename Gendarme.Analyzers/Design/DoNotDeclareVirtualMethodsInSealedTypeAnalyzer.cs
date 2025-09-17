namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotDeclareVirtualMethodsInSealedTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotDeclareVirtualMethodsInSealedTypeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotDeclareVirtualMethodsInSealedTypeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotDeclareVirtualMethodsInSealedTypeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotDeclareVirtualMethodsInSealedType,
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
        if (!namedType.IsSealed) return;

        // Check for virtual methods
        foreach (var member in namedType.GetMembers())
        {
            if (member is IMethodSymbol { IsVirtual: true, IsAbstract: false } method)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    method.Locations[0],
                    namedType.Name,
                    method.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}