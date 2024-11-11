using Gendarme.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseIsOperatorAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseIsOperatorTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseIsOperatorMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseIsOperatorDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseIsOperator,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze binary operations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeBinaryOperation, OperationKind.BinaryOperator);
    }

    private void AnalyzeBinaryOperation(OperationAnalysisContext context)
    {
        var operation = (IBinaryOperation)context.Operation;

        // Look for patterns like (obj as Type) != null
        if (operation is
            not
            {
                OperatorKind: BinaryOperatorKind.NotEquals,
                RightOperand.ConstantValue:
                {
                    HasValue: true,
                    Value: null
                }
            }
            || operation.LeftOperand is not IConversionOperation { OperatorMethod: null } conversion
            || !conversion.IsUnboxingConversion()
            || !conversion.Conversion.IsReference)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax.ToString());
        context.ReportDiagnostic(diagnostic);
    }
}