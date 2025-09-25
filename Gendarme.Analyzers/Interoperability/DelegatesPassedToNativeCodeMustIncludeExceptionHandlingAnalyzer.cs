namespace Gendarme.Analyzers.Interoperability;

/// <summary>
/// Every delegate which is passed to <c>native code</c> must include an exception block which spans the entire method and has a catch-all handler.
/// </summary>
/// <example>
/// Bad example:
/// <code>
/// public void NativeCallback()
/// {
///     Console.WriteLine("{0}", 1);
/// }
/// </code>
/// Good example:
/// <code>
/// public void NativeCallback()
/// {
///     try {
///         Console.WriteLine("{0}", 1);
///     }
///     catch {
///     }
/// }
/// </code>
/// </example>
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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var dllImportAttribute = compilationContext.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.DllImportAttribute");
            if (dllImportAttribute is null)
            {
                return;
            }

            compilationContext.RegisterOperationAction(operationContext =>
            {
                var delegateCreation = (IDelegateCreationOperation)operationContext.Operation;

                if (delegateCreation.Target is not IMethodReferenceOperation { Method: { } methodSymbol })
                {
                    return;
                }

                if (!MethodIsPassedToPInvoke(delegateCreation, operationContext, dllImportAttribute))
                {
                    return;
                }

                var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxReference is null)
                {
                    return;
                }

                if (syntaxReference.GetSyntax(operationContext.CancellationToken) is not MethodDeclarationSyntax methodSyntax)
                {
                    return;
                }

                if (MethodHasCatchAllExceptionHandling(methodSyntax))
                {
                    return;
                }

                var diagnostic = Diagnostic.Create(Rule, methodSyntax.Identifier.GetLocation(), methodSymbol.Name);
                operationContext.ReportDiagnostic(diagnostic);
            }, OperationKind.DelegateCreation);
        });
    }

    private static bool MethodIsPassedToPInvoke(
        IDelegateCreationOperation delegateCreation,
        OperationAnalysisContext context,
        INamedTypeSymbol dllImportAttribute)
    {
        if (delegateCreation.Parent is not IArgumentOperation argumentOperation)
        {
            return false;
        }

        if (argumentOperation.Parent is not IInvocationOperation invocation)
        {
            return false;
        }

        var targetMethod = invocation.TargetMethod;
        if (targetMethod is null)
        {
            return false;
        }

        return targetMethod.GetAttributes()
            .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, dllImportAttribute));
    }

    private static bool MethodHasCatchAllExceptionHandling(MethodDeclarationSyntax methodSyntax)
    {
        var body = methodSyntax.Body;
        if (body is null)
        {
            return false;
        }

        if (body.Statements is not [TryStatementSyntax tryStatement])
        {
            return false;
        }

        return tryStatement.Catches.Any(catchClause => catchClause.Declaration is null);
    }
}
