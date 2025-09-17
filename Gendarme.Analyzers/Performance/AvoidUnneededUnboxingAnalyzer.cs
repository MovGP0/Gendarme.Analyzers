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
        // Analyze methods
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationBlockStartAction(OnOperationBlockStart);
    }

    private void OnOperationBlockStart(OperationBlockStartAnalysisContext context)
    {
        var unboxedValues = new Dictionary<ILocalSymbol, int>();

        context.RegisterOperationAction(operationContext =>
        {
            var unboxing = (IConversionOperation)operationContext.Operation;

            if (unboxing.Operand.Type.SpecialType == SpecialType.System_Object
                && unboxing.Type.IsValueType
                && unboxing.OperatorMethod == null
                && unboxing.IsUnboxingConversion()
                && unboxing.Operand.Type?.IsValueType == false
                && unboxing.Type?.IsValueType == true)
            {
                if (unboxing.Operand is ILocalReferenceOperation local)
                {
                    if (!unboxedValues.TryAdd(local.Local, 1))
                    {
                        unboxedValues[local.Local]++;
                        if (unboxedValues[local.Local] == 2)
                        {
                            var diagnostic = Diagnostic.Create(Rule, unboxing.Syntax.GetLocation(), unboxing.Type.ToDisplayString());
                            operationContext.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }, OperationKind.Conversion);
    }
}