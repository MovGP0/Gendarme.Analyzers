using Microsoft.CodeAnalysis.Operations;

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
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            var disposableType = startContext.Compilation.GetTypeByMetadataName("System.IDisposable");
            var gcType = startContext.Compilation.GetTypeByMetadataName("System.GC");

            if (disposableType is null || gcType is null)
            {
                return;
            }

            startContext.RegisterOperationBlockStartAction(blockStartContext =>
            {
                if (blockStartContext.OwningSymbol is not IMethodSymbol methodSymbol)
                {
                    return;
                }

                if (!IsDisposeCandidate(methodSymbol) || methodSymbol.ContainingType is not { } containingType)
                {
                    return;
                }

                if (!ImplementsDisposable(containingType, disposableType) || !HasFinalizer(containingType))
                {
                    return;
                }

                var callsSuppressFinalize = false;

                blockStartContext.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;

                    if (invocation.TargetMethod is { Name: nameof(GC.SuppressFinalize) } target &&
                        SymbolEqualityComparer.Default.Equals(target.ContainingType, gcType))
                    {
                        callsSuppressFinalize = true;
                    }
                }, OperationKind.Invocation);

                blockStartContext.RegisterOperationBlockEndAction(endContext =>
                {
                    if (!callsSuppressFinalize)
                    {
                        var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], containingType.Name);
                        endContext.ReportDiagnostic(diagnostic);
                    }
                });
            });
        });
    }

    private static bool IsDisposeCandidate(IMethodSymbol method)
        => method is { Name: "Dispose", Parameters.Length: 0 };

    private static bool ImplementsDisposable(INamedTypeSymbol type, INamedTypeSymbol disposableType)
        => type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, disposableType));

    private static bool HasFinalizer(INamedTypeSymbol type)
        => type.GetMembers().OfType<IMethodSymbol>().Any(m => m.MethodKind == MethodKind.Destructor);
}
