namespace Gendarme.Analyzers.Smells;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidCodeDuplicatedInSiblingClassesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidCodeDuplicatedInSiblingClassesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidCodeDuplicatedInSiblingClassesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidCodeDuplicatedInSiblingClassesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidCodeDuplicatedInSiblingClasses,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze class declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        var baseClassToMethods = new Dictionary<INamedTypeSymbol, List<(INamedTypeSymbol, MethodDeclarationSyntax)>>();

        context.RegisterSymbolAction(symbolContext =>
        {
            var classSymbol = (INamedTypeSymbol)symbolContext.Symbol;

            if (classSymbol.BaseType != null && classSymbol.BaseType.SpecialType != SpecialType.System_Object)
            {
                if (!baseClassToMethods.ContainsKey(classSymbol.BaseType))
                {
                    baseClassToMethods[classSymbol.BaseType] = [];
                }

                var methods = classSymbol.GetMembers().OfType<IMethodSymbol>()
                    .Where(m => m.DeclaringSyntaxReferences.Length > 0 && !m.IsAbstract)
                    .Select(m => (classSymbol, m.DeclaringSyntaxReferences[0].GetSyntax() as MethodDeclarationSyntax));

                baseClassToMethods[classSymbol.BaseType].AddRange(methods);
            }
        }, SymbolKind.NamedType);

        context.RegisterCompilationEndAction(compilationContext =>
        {
            foreach (var kvp in baseClassToMethods)
            {
                var methods = kvp.Value;
                if (methods is null)
                {
                    continue;
                }

                var duplicates = FindDuplicates(methods.Select(m => m.Item2).ToList());

                foreach (var method in duplicates)
                {
                    var diagnostic = Diagnostic.Create(Rule, method.Identifier.GetLocation());
                    compilationContext.ReportDiagnostic(diagnostic);
                }
            }
        });
    }

    private static IEnumerable<MethodDeclarationSyntax> FindDuplicates(List<MethodDeclarationSyntax> methods)
    {
        var duplicates = new List<MethodDeclarationSyntax>();

        for (int i = 0; i < methods.Count; i++)
        {
            for (int j = i + 1; j < methods.Count; j++)
            {
                var body1 = methods[i].Body?.ToString() ?? methods[i].ExpressionBody?.Expression.ToString();
                var body2 = methods[j].Body?.ToString() ?? methods[j].ExpressionBody?.Expression.ToString();

                if (body1 != null && body1 == body2)
                {
                    duplicates.Add(methods[i]);
                    duplicates.Add(methods[j]);
                }
            }
        }

        return duplicates.Distinct();
    }
}