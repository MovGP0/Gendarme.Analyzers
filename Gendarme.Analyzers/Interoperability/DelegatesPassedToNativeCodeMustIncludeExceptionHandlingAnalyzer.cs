using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Interoperability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DelegatesPassedToNativeCodeMustIncludeExceptionHandlingMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DelegatesPassedToNativeCodeMustIncludeExceptionHandlingDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DelegatesPassedToNativeCodeMustIncludeExceptionHandling,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var dllImportAttribute = compilationContext.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.DllImportAttribute");

            if (dllImportAttribute == null)
                return;

            compilationContext.RegisterOperationAction(operationContext =>
            {
                var delegateCreation = (IDelegateCreationOperation)operationContext.Operation;

                if (delegateCreation.Target is not IMethodReferenceOperation targetMethod)
                    return;

                var methodSymbol = targetMethod.Method;

                if (MethodIsPassedToPInvoke(delegateCreation, operationContext))
                {
                    // Check if method has a try-catch block that spans the entire method
                    if (methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                        return;

                    var hasCatchAll = MethodHasCatchAllExceptionHandling(methodSyntax);

                    if (!hasCatchAll)
                    {
                        var diagnostic = Diagnostic.Create(Rule, methodSyntax.Identifier.GetLocation(), methodSymbol.Name);
                        operationContext.ReportDiagnostic(diagnostic);
                    }
                }
            }, OperationKind.DelegateCreation);
        });
    }

    private static bool MethodIsPassedToPInvoke(IDelegateCreationOperation delegateCreation, OperationAnalysisContext context)
    {
        if (delegateCreation.Parent is not IArgumentOperation parentInvocation)
            return false;

        if (parentInvocation.Parent is not IInvocationOperation targetMethod)
            return false;

        var methodSymbol = targetMethod.TargetMethod;

        var hasDllImport = methodSymbol.GetAttributes().Any(attr => attr.AttributeClass.ToString() == "System.Runtime.InteropServices.DllImportAttribute");
        return hasDllImport;
    }

    private static bool MethodHasCatchAllExceptionHandling(MethodDeclarationSyntax methodSyntax)
    {
        var body = methodSyntax.Body;
        if (body == null)
            return false;

        if (body.Statements is not [TryStatementSyntax tryStatement])
            return false;

        var catchClauses = tryStatement.Catches;
        if (!catchClauses.Any())
            return false;

        // Check for a catch-all clause
        return catchClauses.Any(catchClause => catchClause.Declaration == null);
    }
}