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

    private const int MinimumFieldCount = 5;
    private const int MinimumMethodCount = 5;
    private const double SuccessLowerLimit = 0.5;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        if (typeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        var fields = typeSymbol.GetMembers().OfType<IFieldSymbol>().Where(field => !field.IsStatic).ToList();
        var methods = typeSymbol.GetMembers().OfType<IMethodSymbol>()
            .Where(method => method is { IsStatic: false, MethodKind: MethodKind.Ordinary })
            .ToList();

        if (fields.Count < MinimumFieldCount || methods.Count < MinimumMethodCount)
        {
            return;
        }

        var location = typeSymbol.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var methodPairs = 0;
        var disjointMethodPairs = 0;

        for (var i = 0; i < methods.Count; i++)
        {
            for (var j = i + 1; j < methods.Count; j++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                methodPairs++;

                var fieldsUsedByMethod1 = GetFieldsUsedByMethod(methods[i], context.Compilation, context.CancellationToken);
                var fieldsUsedByMethod2 = GetFieldsUsedByMethod(methods[j], context.Compilation, context.CancellationToken);

                if (!fieldsUsedByMethod1.Intersect(fieldsUsedByMethod2).Any())
                {
                    disjointMethodPairs++;
                }
            }
        }

        if (methodPairs == 0)
        {
            return;
        }

        var levelOfComplexity = (double)disjointMethodPairs / methodPairs;

        if (levelOfComplexity > SuccessLowerLimit)
        {
            var diagnostic = Diagnostic.Create(Rule, location, typeSymbol.Name, levelOfComplexity.ToString("F2"));
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static ImmutableHashSet<string> GetFieldsUsedByMethod(IMethodSymbol methodSymbol, Compilation compilation, CancellationToken cancellationToken)
    {
        var builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (var syntaxReference in methodSymbol.DeclaringSyntaxReferences)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (syntaxReference.GetSyntax(cancellationToken) is not MethodDeclarationSyntax methodSyntax)
            {
                continue;
            }

            var semanticModel = compilation.GetSemanticModel(methodSyntax.SyntaxTree);

            foreach (var identifier in methodSyntax.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                var symbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol as IFieldSymbol;
                if (symbol is { IsStatic: false } field &&
                    SymbolEqualityComparer.Default.Equals(field.ContainingType, methodSymbol.ContainingType))
                {
                    builder.Add(field.Name);
                }
            }
        }

        return builder.ToImmutable();
    }
}
