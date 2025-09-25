using Gendarme.Analyzers.Extensions;

namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule looks for complex cast operations (e.g. an <c>aswith</c> a null check)
/// that can be simplified using the is operator (C# syntax).
/// </summary>
/// <remarks>
/// In some case a compiler, like <c>[g]mcs</c>, can optimize the code and generate IL identical to an <c>is</c> operator.
/// </remarks>
/// <example>
/// Bad example:
/// <code language="C#">
/// bool is_my_type = (my_instance as MyType) != null;
/// </code>
/// Bad example:
/// <code language="C#">
/// bool is_my_type = (my_instance as MyType) is not null;
/// </code>
/// Good example:
/// <code language="C#">
/// bool is_my_type = (my_instance is MyType);
/// </code>
/// </example>
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