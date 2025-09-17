using Gendarme.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidRepetitiveCastsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidRepetitiveCastsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidRepetitiveCastsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidRepetitiveCastsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidRepetitiveCasts,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method bodies
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationBlockStartAction(OnOperationBlockStart);
    }

    private void OnOperationBlockStart(OperationBlockStartAnalysisContext context)
    {
        var castExpressions = new Dictionary<(IOperation, ITypeSymbol), int>();

        context.RegisterOperationAction(operationContext =>
        {
            var conversion = (IConversionOperation)operationContext.Operation;

            if (conversion.Type is not { } type)
            {
                return;
            }

            var key = (conversion.Operand, type);

            if (castExpressions.TryAdd(key, 1))
            {
                return;
            }

            castExpressions[key]++;
            if (castExpressions[key] == 2)
            {
                var diagnostic = Diagnostic.Create(Rule, conversion.Syntax.GetLocation(), conversion.Operand.Syntax.ToString(), conversion.Type.ToDisplayString());
                operationContext.ReportDiagnostic(diagnostic);
            }
        }, OperationKind.Conversion);
    }
}
