using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotShortCircuitCertificateCheckAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotShortCircuitCertificateCheckTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotShortCircuitCertificateCheckMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotShortCircuitCertificateCheckDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotShortCircuitCertificateCheck,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<string> CertificateValidationMethodNames = ImmutableHashSet.Create(
        "CheckValidationResult",
        "CertificateValidationCallback"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method bodies
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeMethodBody, OperationKind.MethodBody);
    }

    private void AnalyzeMethodBody(OperationAnalysisContext context)
    {
        var methodBody = (IMethodBodyOperation)context.Operation;
        var methodSymbol = methodBody.SemanticModel.GetDeclaredSymbol(methodBody.Syntax) as IMethodSymbol;

        if (methodSymbol == null)
            return;

        // Check if the method is a certificate validation callback
        if (!CertificateValidationMethodNames.Contains(methodSymbol.Name))
            return;

        // Check if the method unconditionally returns true
        var returns = methodBody.Descendants().OfType<IReturnOperation>().ToList();

        foreach (var returnOp in returns)
        {
            var constantValue = returnOp.ReturnedValue?.ConstantValue;

            if (constantValue is not { Value: true })
            {
                continue;
            }

            // Unconditionally returns true
            var diagnostic = Diagnostic.Create(Rule, returnOp.Syntax.GetLocation(), methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}