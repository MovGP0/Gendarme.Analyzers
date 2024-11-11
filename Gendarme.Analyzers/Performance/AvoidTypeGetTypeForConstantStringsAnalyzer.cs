using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidTypeGetTypeForConstantStringsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidTypeGetTypeForConstantStringsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidTypeGetTypeForConstantStringsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidTypeGetTypeForConstantStringsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidTypeGetTypeForConstantStrings,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze invocation operations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInvocationOperation, OperationKind.Invocation);
    }

    private void AnalyzeInvocationOperation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;

        // Check if the method is Type.GetType(string)
        if (invocation.TargetMethod.Name == "GetType" &&
            invocation.TargetMethod.ContainingType.ToDisplayString() == "System.Type" &&
            invocation.Arguments.Length == 1 &&
            invocation.Arguments[0].Value.ConstantValue.HasValue &&
            invocation.Arguments[0].Value.Type.SpecialType == SpecialType.System_String)
        {
            var typeName = invocation.Arguments[0].Value.ConstantValue.Value as string;

            if (!string.IsNullOrEmpty(typeName))
            {
                // Suggest using typeof instead
                var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), typeName, typeName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}