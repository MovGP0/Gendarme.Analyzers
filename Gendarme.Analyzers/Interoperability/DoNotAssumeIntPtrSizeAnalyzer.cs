using Gendarme.Analyzers.Extensions;

namespace Gendarme.Analyzers.Interoperability;

/// <summary>
/// This rule checks for code which casts an <c>IntPtr</c> or <c>UIntPtr</c> into a 32-bit (or smaller) value.
/// It will also check if memory read with the <c>Marshal.ReadInt32</c> and <c>Marshal.ReadInt64</c> methods is being cast into an <c>IntPtr</c> or <c>UIntPtr</c>.
/// <c>IntPtr</c> is generally used to reference a memory location and down-casting them to 32-bits will make the code fail on 64-bit CPUs.
/// </summary>
/// <example>
/// Bad example (cast):
/// <code language="c#">
///   int ptr = dest.ToInt32();
///   for (int i = 0; i &lt; 16; i++) {
///       Marshal.StructureToPtr(this, (IntPtr)ptr, false);
///       ptr += 4;
///   }
/// </code>
/// Bad example (<c>Marshal.Read*</c>):
/// <code language="c#">
/// // that won't work on 64 bits platforms
/// IntPtr p = (IntPtr) Marshal.ReadInt32(p);
/// </code>
/// Good example (cast):
/// <code language="c#">
/// long ptr = dest.ToInt64();
/// for (int i = 0; i &lt; 16; i++) {
///     Marshal.StructureToPtr(this, (IntPtr) ptr, false);
///     ptr += IntPtr.Size;
/// }
/// </code>
/// Good example (<c>Marshal.Read*</c>):
/// <code language="c#">
/// IntPtr p = (IntPtr)Marshal.ReadIntPtr(p);
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotAssumeIntPtrSizeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotAssumeIntPtrSizeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotAssumeIntPtrSizeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotAssumeIntPtrSizeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotAssumeIntPtrSize,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private static readonly string[] MarshalReadMethods = ["ReadInt32", "ReadInt64"];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeCastExpression, SyntaxKind.CastExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeCastExpression(SyntaxNodeAnalysisContext context)
    {
        var castExpression = (CastExpressionSyntax)context.Node;

        var fromType = context.SemanticModel.GetTypeInfo(castExpression.Expression).Type;
        var toType = context.SemanticModel.GetTypeInfo(castExpression.Type).Type;

        if (fromType == null || toType == null)
            return;

        if ((fromType.IsIntPtrType() || fromType.IsUIntPtrType()) && toType.Is32BitOrSmaller())
        {
            var diagnostic = Diagnostic.Create(Rule, castExpression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol symbol)
            return;

        if (symbol.ContainingType.ToString() == "System.Runtime.InteropServices.Marshal" &&
            MarshalReadMethods.Contains(symbol.Name))
        {
            if (invocationExpression.Parent is CastExpressionSyntax parentCast)
            {
                var toType = context.SemanticModel.GetTypeInfo(parentCast.Type).Type;
                if (toType != null && (toType.IsIntPtrType() || toType.IsUIntPtrType()))
                {
                    var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    
}