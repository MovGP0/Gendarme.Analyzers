using Gendarme.Analyzers.Extensions;
using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule fires if multiple casts are done on the same value, for the same type.
/// Casts are expensive so reducing them, by changing the logic or caching the result, can help performance.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// foreach (object o in list) {
///     // first cast (is)
///     if (o is ICollection) {
///         // second cast (as) if item implements ICollection
///         Process (o as ICollection);
///     }
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// foreach (object o in list) {
///     // a single cast (as) per item
///     ICollection c = (o as ICollection);
///     if (c != null) {
///         SingleCast (c);
///     }
/// }
/// </code>
/// Bad example:
/// <code language="C#">
/// // first cast (is):
/// if (o is IDictionary) {
///     // second cast if item implements IDictionary:
///     Process ((IDictionary) o);
///     // first cast (is):
/// } else if (o is ICollection) {
///     // second cast if item implements ICollection:
///     Process ((ICollection) o);
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// // a single cast (as) per item
/// IDictionary dict;
/// ICollection col;
/// if ((dict = o as IDictionary) != null) {
///     SingleCast (dict);
/// } else if ((col = o as ICollection) != null) {
///     SingleCast (col);
/// }
/// </code>
/// </example>
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
