namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewInconsistentIdentityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewInconsistentIdentity_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewInconsistentIdentity_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewInconsistentIdentity_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewInconsistentIdentity,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        var methods = namedTypeSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .ToImmutableArray();

        var equalsMethods = methods.Where(m => m is { Name: "Equals", Parameters.Length: 1 }).ToList();
        var getHashCodeMethods = methods.Where(m => m is { Name: "GetHashCode", Parameters.Length: 0 }).ToList();
        var compareToMethods = methods.Where(m => m is { Name: "CompareTo", Parameters.Length: 1 }).ToList();

        foreach (var method in equalsMethods)
        {
            if (UsesSameFields(method, getHashCodeMethods, compareToMethods, context.Compilation))
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, method.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool UsesSameFields(
        IMethodSymbol equalsMethod,
        IEnumerable<IMethodSymbol> getHashCodeMethods,
        IEnumerable<IMethodSymbol> compareToMethods,
        Compilation compilation)
    {
        var fieldsUsedInEquals = GetFieldsUsed(equalsMethod, compilation);
        var fieldsUsedInGetHashCode = getHashCodeMethods.SelectMany(m => GetFieldsUsed(m, compilation)).ToList();
        var fieldsUsedInCompareTo = compareToMethods.SelectMany(m => GetFieldsUsed(m, compilation)).ToList();

        return fieldsUsedInEquals.SetEquals(fieldsUsedInGetHashCode)
               && fieldsUsedInEquals.SetEquals(fieldsUsedInCompareTo);
    }

    private static HashSet<ISymbol> GetFieldsUsed(IMethodSymbol method, Compilation compilation)
    {
        var methodSyntaxTree = method.DeclaringSyntaxReferences
            .FirstOrDefault()?.SyntaxTree;

        if (methodSyntaxTree == null)
        {
            return [];
        }

        var methodNode = methodSyntaxTree
            .GetRoot()
            .DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text == method.Name);

        if (methodNode == null)
        {
            return [];
        }

        var parentSyntaxTree = method.ContainingType.DeclaringSyntaxReferences
            .FirstOrDefault()
            ?.SyntaxTree;

        if (parentSyntaxTree is null)
        {
            return [];
        }

        var semanticModel = compilation.GetSemanticModel(parentSyntaxTree);

        if (methodNode.Body is null)
        {
            return [];
        }

        return methodNode.Body
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(identifier => semanticModel.GetSymbolInfo(identifier).Symbol)
            .OfType<IFieldSymbol>()
            .ToHashSet<ISymbol>(SymbolEqualityComparer.Default);
    }
}
