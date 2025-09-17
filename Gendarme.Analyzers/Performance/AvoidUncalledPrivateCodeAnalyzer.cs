using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUncalledPrivateCodeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUncalledPrivateCodeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUncalledPrivateCodeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUncalledPrivateCodeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUncalledPrivateCode,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description,
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(StartAnalysis);
    }

    private static void StartAnalysis(CompilationStartAnalysisContext context)
    {
        var calledMethods = new ConcurrentDictionary<IMethodSymbol, byte>(SymbolEqualityComparer.Default);
        var candidateMethods = new ConcurrentBag<IMethodSymbol>();

        context.RegisterOperationAction(operationContext =>
        {
            var invocation = (IInvocationOperation)operationContext.Operation;
            calledMethods[invocation.TargetMethod.OriginalDefinition] = 0;
        }, OperationKind.Invocation);

        context.RegisterSymbolAction(symbolContext =>
        {
            var methodSymbol = (IMethodSymbol)symbolContext.Symbol;

            if (methodSymbol.IsImplicitlyDeclared)
            {
                return;
            }

            if (methodSymbol.MethodKind != MethodKind.Ordinary)
            {
                return;
            }

            candidateMethods.Add(methodSymbol);
        }, SymbolKind.Method);

        context.RegisterCompilationEndAction(compilationContext =>
        {
            var entryPoint = compilationContext.Compilation.GetEntryPoint(compilationContext.CancellationToken);

            foreach (var method in candidateMethods)
            {
                if (!ShouldReport(method, entryPoint, calledMethods))
                {
                    continue;
                }

                var location = method.Locations.FirstOrDefault();
                if (location is null)
                {
                    continue;
                }

                var diagnostic = Diagnostic.Create(Rule, location, method.Name);
                compilationContext.ReportDiagnostic(diagnostic);
            }
        });
    }

    private static bool ShouldReport(
        IMethodSymbol method,
        IMethodSymbol? entryPoint,
        ConcurrentDictionary<IMethodSymbol, byte> calledMethods)
    {
        if (method.DeclaredAccessibility is not (Accessibility.Private or Accessibility.Internal))
        {
            return false;
        }

        if (method.IsAbstract || method.IsVirtual || method.IsOverride)
        {
            return false;
        }

        if (SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, entryPoint))
        {
            return false;
        }

        return !calledMethods.ContainsKey(method.OriginalDefinition);
    }
}
