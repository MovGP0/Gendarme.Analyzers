namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule will fire if a type implements <c>System.IDisposable</c> and has a finalizer (called a destructor in C#),
/// but the Dispose method does not call <c>System.GC.SuppressFinalize</c>.
/// Failing to do this should not cause properly written code to fail,
/// but it does place a non-trivial amount of extra pressure on the garbage collector and on the finalizer thread.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// class BadClass : IDisposable {
///     ~BadClass ()
///     {
///         Dispose (false);
///     }
///  
///     public void Dispose ()
///     {
///         // GC.SuppressFinalize is missing so the finalizer will be called
///         // which puts needless extra pressure on the garbage collector.
///         Dispose (true);
///     }
///  
///     private void Dispose (bool disposing)
///     {
///         if (ptr != IntPtr.Zero) {
///             Free (ptr);
///             ptr = IntPtr.Zero;
///         }
///     }
///  
///     [DllImport ("somelib")]
///     private static extern void Free (IntPtr ptr);
///  
///     private IntPtr ptr;
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// class GoodClass : IDisposable {
///     ~GoodClass ()
///     {
///         Dispose (false);
///     }
///  
///     public void Dispose ()
///     {
///         Dispose (true);
///         GC.SuppressFinalize (this);
///     }
///  
///     private void Dispose (bool disposing)
///     {
///         if (ptr != IntPtr.Zero) {
///             Free (ptr);
///             ptr = IntPtr.Zero;
///         }
///     }
///  
///     [DllImport ("somelib")]
///     private static extern void Free (IntPtr ptr);
///  
///     private IntPtr ptr;
/// }
/// </code>
/// </example>
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
