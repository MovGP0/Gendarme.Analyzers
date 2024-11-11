using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewLinqMethodAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewLinqMethodTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewLinqMethodMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewLinqMethodDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewLinqMethod,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<string> MethodsToReview = ImmutableHashSet.Create(
        "Count",
        "Last"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method invocations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;

        var methodName = invocation.TargetMethod.Name;

        if (!MethodsToReview.Contains(methodName))
            return;

        // Check if the method is from System.Linq.Enumerable
        if (invocation.TargetMethod.ContainingType.ToDisplayString() != "System.Linq.Enumerable")
            return;

        // Suggest alternatives
        var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), methodName);
        context.ReportDiagnostic(diagnostic);
    }
}