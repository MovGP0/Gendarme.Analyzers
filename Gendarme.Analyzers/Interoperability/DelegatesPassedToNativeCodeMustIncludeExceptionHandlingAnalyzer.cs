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

                var targetMethod = delegateCreation.Target as IMethodReferenceOperation;
                if (targetMethod == null)
                    return;

                var methodSymbol = targetMethod.Method;

                if (MethodIsPassedToPInvoke(delegateCreation, operationContext))
                {
                    // Check if method has a try-catch block that spans the entire method
                    var methodSyntax = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
                    if (methodSyntax == null)
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
        var parentInvocation = delegateCreation.Parent as IArgumentOperation;
        if (parentInvocation == null)
            return false;

        var targetMethod = parentInvocation.Parent as IInvocationOperation;
        if (targetMethod == null)
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

        if (body.Statements.Count != 1 || !(body.Statements[0] is TryStatementSyntax tryStatement))
            return false;

        var catchClauses = tryStatement.Catches;
        if (!catchClauses.Any())
            return false;

        // Check for a catch-all clause
        return catchClauses.Any(catchClause => catchClause.Declaration == null);
    }
}