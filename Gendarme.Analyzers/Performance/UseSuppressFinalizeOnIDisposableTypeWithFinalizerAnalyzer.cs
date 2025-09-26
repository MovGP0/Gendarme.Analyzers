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

        context.RegisterCompilationStartAction(static compilationContext =>
        {
            var disposableType = compilationContext.Compilation.GetTypeByMetadataName("System.IDisposable");
            var gcType = compilationContext.Compilation.GetTypeByMetadataName(typeof(GC).FullName!);

            if (disposableType is null || gcType is null)
            {
                return;
            }

            compilationContext.RegisterOperationBlockStartAction(blockStartContext =>
            {
                if (blockStartContext.OwningSymbol is not IMethodSymbol method)
                {
                    return;
                }

                if (!IsDisposeMethod(method, disposableType))
                {
                    return;
                }

                var containingType = method.ContainingType;
                if (!ImplementsDisposable(containingType, disposableType) || !HasFinalizer(containingType))
                {
                    return;
                }

                var callsSuppressFinalize = false;

                blockStartContext.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;

                    if (!SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, gcType))
                    {
                        return;
                    }

                    if (!string.Equals(invocation.TargetMethod.Name, nameof(GC.SuppressFinalize), StringComparison.Ordinal))
                    {
                        return;
                    }

                    if (invocation.Arguments.Length != 1)
                    {
                        return;
                    }

                    if (IsThisArgument(invocation.Arguments[0]))
                    {
                        callsSuppressFinalize = true;
                    }
                }, OperationKind.Invocation);

                blockStartContext.RegisterOperationBlockEndAction(endContext =>
                {
                    if (callsSuppressFinalize)
                    {
                        return;
                    }

                    var diagnosticLocation = GetTypeLocation(containingType) ?? method.Locations.FirstOrDefault();
                    if (diagnosticLocation is null)
                    {
                        return;
                    }

                    endContext.ReportDiagnostic(Diagnostic.Create(Rule, diagnosticLocation, containingType.Name));
                });
            });
        });
    }

    private static bool IsDisposeMethod(IMethodSymbol method, INamedTypeSymbol disposableType)
    {
        if (method is not { Name: nameof(IDisposable.Dispose), Parameters.IsEmpty: true, ReturnsVoid: true, IsStatic: false })
        {
            return false;
        }

        if (method.MethodKind is MethodKind.ExplicitInterfaceImplementation)
        {
            return method.ExplicitInterfaceImplementations.Any(explicitImplementation =>
                SymbolEqualityComparer.Default.Equals(explicitImplementation.ContainingType, disposableType));
        }

        return true;
    }

    private static bool ImplementsDisposable(INamedTypeSymbol type, INamedTypeSymbol disposableType) =>
        type.AllInterfaces.Any(@interface => SymbolEqualityComparer.Default.Equals(@interface, disposableType));

    private static bool HasFinalizer(INamedTypeSymbol type) =>
        type.GetMembers().OfType<IMethodSymbol>().Any(member => member.MethodKind == MethodKind.Destructor);

    private static bool IsThisArgument(IArgumentOperation argument)
    {
        var value = Unwrap(argument.Value);
        return value is IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance };
    }

    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    private static Location? GetTypeLocation(INamedTypeSymbol type) =>
        type.Locations.FirstOrDefault(location => location.IsInSource);
}