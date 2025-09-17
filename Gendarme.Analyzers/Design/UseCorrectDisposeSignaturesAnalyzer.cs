using System.Text;

namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCorrectDisposeSignaturesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseCorrectDisposeSignaturesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseCorrectDisposeSignaturesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseCorrectDisposeSignaturesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseCorrectDisposeSignatures,
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
        if (!namedType.AllInterfaces.Any(i => i.ToDisplayString() == "System.IDisposable")) 
            return;

        // Check for typical Dispose pattern:
        // 1) public non-virtual Dispose()
        // 2) protected virtual Dispose(bool)
        // 3) finalizer if unsealed
        // etc.

        bool hasPublicDispose = false;
        bool hasProtectedVirtualDisposeBool = false;

        var methods = namedType.GetMembers().OfType<IMethodSymbol>();
        var sbProblems = new StringBuilder();

        foreach (var m in methods)
        {
            if (m.Name == "Dispose")
            {
                // No parameters => should be public, not virtual
                if (m.Parameters.Length == 0)
                {
                    if (m is { DeclaredAccessibility: Accessibility.Public, IsVirtual: false })
                    {
                        hasPublicDispose = true;
                    }
                    else
                    {
                        sbProblems.Append("[Dispose() should be public non-virtual]; ");
                    }
                }
                // One bool param => should be protected virtual if the type is not sealed
                else if (m.Parameters is [{ Type.SpecialType: SpecialType.System_Boolean }])
                {
                    if (namedType.IsSealed)
                    {
                        // a sealed class can keep this method private or protected
                        // but typically it's private, not necessarily virtual
                    }
                    else
                    {
                        if (m is { DeclaredAccessibility: Accessibility.Protected, IsVirtual: true })
                        {
                            hasProtectedVirtualDisposeBool = true;
                        }
                        else
                        {
                            sbProblems.Append("[Dispose(bool) should be protected virtual for unsealed]; ");
                        }
                    }
                }
            }
        }

        // If we didn't find public Dispose() or protected virtual Dispose(bool) in an unsealed type
        if (!hasPublicDispose)
        {
            sbProblems.Append("[Missing public Dispose()]; ");
        }
        if (!namedType.IsSealed && !hasProtectedVirtualDisposeBool)
        {
            sbProblems.Append("[Missing protected virtual Dispose(bool)]; ");
        }

        if (sbProblems.Length > 0)
        {
            var diag = Diagnostic.Create(
                Rule,
                namedType.Locations.FirstOrDefault(),
                namedType.Name,
                sbProblems.ToString());
            context.ReportDiagnostic(diag);
        }
    }
}