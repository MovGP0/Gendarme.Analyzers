namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotLockOnWeakIdentityObjectsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Message;
    private static readonly LocalizableString Description = Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotLockOnWeakIdentityObjects,
        Title,
        MessageFormat,
        Category.Concurrency,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LockStatement);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var lockStatement = (LockStatementSyntax)context.Node;
        var expression = lockStatement.Expression;

        var type = context.SemanticModel.GetTypeInfo(expression).Type;
        if (type == null)
            return;

        var weakIdentities = new[]
        {
            "System.String",
            "System.MarshalByRefObject",
            "System.OutOfMemoryException",
            "System.Reflection.MemberInfo",
            "System.Reflection.ParameterInfo",
            "System.ExecutionEngineException",
            "System.StackOverflowException",
            "System.Threading.Thread"
        };

        if (weakIdentities.Contains(type.ToString()))
        {
            var diagnostic = Diagnostic.Create(Rule, expression.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}