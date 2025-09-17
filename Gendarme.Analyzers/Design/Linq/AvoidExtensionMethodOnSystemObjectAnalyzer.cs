namespace Gendarme.Analyzers.Design.Linq;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidExtensionMethodOnSystemObjectAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidExtensionMethodOnSystemObject_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidExtensionMethodOnSystemObject_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidExtensionMethodOnSystemObject_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidExtensionMethodOnSystemObject,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        if (methodDeclaration.ParameterList.Parameters.Count <= 0 ||
            !methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
            !methodDeclaration.ParameterList.Parameters[0].Modifiers.Any(SyntaxKind.ThisKeyword))
        {
            return;
        }

        var firstParameter = methodDeclaration.ParameterList.Parameters[0];

        if (firstParameter.Type is not {} firstParameterType)
        {
            return;
        }
    
        var parameterType = context.SemanticModel.GetTypeInfo(firstParameterType).Type;

        if (parameterType?.SpecialType != SpecialType.System_Object)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, firstParameter.GetLocation(), methodDeclaration.Identifier.Text);
        context.ReportDiagnostic(diagnostic);
    }
}
