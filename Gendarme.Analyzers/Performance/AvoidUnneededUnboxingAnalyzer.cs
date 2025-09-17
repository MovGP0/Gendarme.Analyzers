using Gendarme.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnneededUnboxingAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnneededUnboxingTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnneededUnboxingMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnneededUnboxingDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnneededUnboxing,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationBlockStartAction(OnOperationBlockStart);
    }

    private static void OnOperationBlockStart(OperationBlockStartAnalysisContext context)
    {
        var unboxedValues = new Dictionary<ILocalSymbol, int>(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            var unboxing = (IConversionOperation)operationContext.Operation;

            if (unboxing.Operand.Type is not { } operandType ||
                unboxing.Type is not { } targetType ||
                operandType.SpecialType != SpecialType.System_Object ||
                !targetType.IsValueType ||
                operandType.IsValueType ||
                unboxing.OperatorMethod is not null ||
                !unboxing.IsUnboxingConversion())
            {
                return;
            }

            if (unboxing.Operand is not ILocalReferenceOperation { Local: { } localSymbol })
            {
                return;
            }

            if (!unboxedValues.TryGetValue(localSymbol, out var count))
            {
                unboxedValues[localSymbol] = 1;
                return;
            }

            count++;
            unboxedValues[localSymbol] = count;

            if (count == 2)
            {
                var diagnostic = Diagnostic.Create(Rule, unboxing.Syntax.GetLocation(), targetType.ToDisplayString());
                operationContext.ReportDiagnostic(diagnostic);
            }
        }, OperationKind.Conversion);
    }
}
