using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Portability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MonoCompatibilityReviewAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MonoCompatibilityReviewTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MonoCompatibilityReviewMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MonoCompatibilityReviewDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MonoCompatibilityReview,
        Title,
        MessageFormat,
        Category.Portability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<string> IncompatibleMethods = ImmutableHashSet.Create(
        "System.Drawing.Bitmap.SetResolution",
        "System.Drawing.Graphics.DrawArc"
        // Add other methods known to be incompatible
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method invocations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocationOperation = (IInvocationOperation)context.Operation;

        var methodFullName = invocationOperation.TargetMethod.ContainingType.ToDisplayString() + "." + invocationOperation.TargetMethod.Name;

        if (IncompatibleMethods.Contains(methodFullName))
        {
            var diagnostic = Diagnostic.Create(Rule, invocationOperation.Syntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}