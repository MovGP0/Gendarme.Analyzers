namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class InstantiateArgumentExceptionCorrectlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.InstantiateArgumentExceptionCorrectlyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.InstantiateArgumentExceptionCorrectlyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.InstantiateArgumentExceptionCorrectlyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.InstantiateArgumentExceptionCorrectly,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private static readonly ImmutableDictionary<string, int> ParameterPositions = new Dictionary<string, int>
    {
        { "System.ArgumentException", 1 },
        { "System.ArgumentNullException", 0 },
        { "System.ArgumentOutOfRangeException", 0 },
        { "System.DuplicateWaitObjectException", 0 }
    }.ToImmutableDictionary();

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
    }

    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax objectCreation
            || objectCreation.ArgumentList is null
            || context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol is not INamedTypeSymbol typeSymbol
            || typeSymbol.ToString() is not { } typeName
            || !ParameterPositions.ContainsKey(typeName)
            || context.SemanticModel.GetSymbolInfo(objectCreation).Symbol is not IMethodSymbol constructorSymbol)
        {
            return;
        }

        var expectedPosition = ParameterPositions[typeName];
        if (constructorSymbol.Parameters.Length <= expectedPosition)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, objectCreation.GetLocation(), typeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}