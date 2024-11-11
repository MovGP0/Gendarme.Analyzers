using Gendarme.Analyzers.Extensions;

namespace Gendarme.Analyzers.Interoperability;

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
        var symbol = context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol as IMethodSymbol;

        if (symbol == null)
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