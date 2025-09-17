namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotLockOnWeakIdentityObjectsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Description), Strings.ResourceManager, typeof(Strings));

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