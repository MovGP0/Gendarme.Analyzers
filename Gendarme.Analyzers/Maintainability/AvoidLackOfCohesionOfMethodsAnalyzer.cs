namespace Gendarme.Analyzers.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidLackOfCohesionOfMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidLackOfCohesionOfMethodsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidLackOfCohesionOfMethodsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidLackOfCohesionOfMethodsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidLackOfCohesionOfMethods,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int MinimumFieldCount = 5; // Configurable
    private const int MinimumMethodCount = 5; // Configurable
    private const double SuccessLowerLimit = 0.5; // Configurable

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        if (typeSymbol.TypeKind != TypeKind.Class)
            return;

        var fields = typeSymbol.GetMembers().OfType<IFieldSymbol>().Where(f => !f.IsStatic).ToList();
        var methods = typeSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m is { IsStatic: false, MethodKind: MethodKind.Ordinary }).ToList();

        if (fields.Count < MinimumFieldCount || methods.Count < MinimumMethodCount)
            return;

        // Use the semantic model from the context's compilation
        var semanticModel = context.Compilation.GetSemanticModel(context.Symbol.DeclaringSyntaxReferences.First().SyntaxTree);

        // Calculate the lack of cohesion of methods (LCOM) metric
        int methodPairs = 0;
        int disjointMethodPairs = 0;

        for (int i = 0; i < methods.Count; i++)
        {
            for (int j = i + 1; j < methods.Count; j++)
            {
                methodPairs++;

                var fieldsUsedByMethod1 = GetFieldsUsedByMethod(methods[i], semanticModel);
                var fieldsUsedByMethod2 = GetFieldsUsedByMethod(methods[j], semanticModel);

                if (!fieldsUsedByMethod1.Intersect(fieldsUsedByMethod2).Any())
                {
                    disjointMethodPairs++;
                }
            }
        }

        if (methodPairs == 0)
            return;

        double levelOfComplexity = (double)disjointMethodPairs / methodPairs;

        if (levelOfComplexity > SuccessLowerLimit)
        {
            var diagnostic = Diagnostic.Create(Rule, typeSymbol.Locations[0], typeSymbol.Name, levelOfComplexity.ToString("F2"));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static ImmutableHashSet<string> GetFieldsUsedByMethod(IMethodSymbol methodSymbol, SemanticModel semanticModel)
    {
        var fieldsUsed = ImmutableHashSet.CreateBuilder<string>();

        foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not MethodDeclarationSyntax methodSyntax)
                continue;

            var fieldAccesses = methodSyntax.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Select(id => semanticModel.GetSymbolInfo(id).Symbol as IFieldSymbol)
                .Where(field => field is { IsStatic: false } && field.ContainingType.Equals(methodSymbol.ContainingType));

            foreach (var field in fieldAccesses)
            {
                fieldsUsed.Add(field.Name);
            }
        }

        return fieldsUsed.ToImmutable();
    }
}