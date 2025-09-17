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
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method symbols
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        var calledMethods = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            var operation = operationContext.Operation;

            if (operation is IInvocationOperation invocation)
            {
                var targetMethod = invocation.TargetMethod;
                calledMethods.Add(targetMethod.OriginalDefinition);
            }
        }, OperationKind.Invocation);

        context.RegisterCompilationEndAction(compilationContext =>
        {
            var compilation = compilationContext.Compilation;
            var allMethods = compilation.SyntaxTrees
                .SelectMany(syntaxTree => syntaxTree.GetRoot().DescendantNodes())
                .OfType<MethodDeclarationSyntax>()
                .Select(methodDecl => compilation.GetSemanticModel(methodDecl.SyntaxTree).GetDeclaredSymbol(methodDecl))
                .OfType<IMethodSymbol>();

            var entryPoint = context.Compilation.GetEntryPoint(context.CancellationToken);
            foreach (var method in allMethods)
            {
                if (method.DeclaredAccessibility is Accessibility.Private or Accessibility.Internal &&
                    !calledMethods.Contains(method.OriginalDefinition) &&
                    !SymbolEqualityComparer.Default.Equals(method, entryPoint))
                {
                    var diagnostic = Diagnostic.Create(Rule, method.Locations[0], method.Name);
                    compilationContext.ReportDiagnostic(diagnostic);
                }
            }
        });
    }
}
