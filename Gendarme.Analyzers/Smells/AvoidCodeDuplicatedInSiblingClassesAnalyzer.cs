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
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        // Thread-safe aggregation across concurrent analysis callbacks
        var baseClassToMethods = new ConcurrentDictionary<INamedTypeSymbol, ConcurrentBag<MethodDeclarationSyntax>>(SymbolEqualityComparer.Default);

        context.RegisterSyntaxNodeAction(syntaxContext =>
        {
            var classDecl = (ClassDeclarationSyntax)syntaxContext.Node;
            if (syntaxContext.SemanticModel.GetDeclaredSymbol(classDecl, syntaxContext.CancellationToken) is not INamedTypeSymbol classSymbol)
                return;

            var baseType = classSymbol.BaseType;
            if (baseType is null || baseType.SpecialType == SpecialType.System_Object)
                return;

            var bag = baseClassToMethods.GetOrAdd(baseType, _ => []);

            foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.Body is null && method.ExpressionBody is null)
                    continue;
                bag.Add(method);
            }
        }, SyntaxKind.ClassDeclaration);

        context.RegisterCompilationEndAction(compilationContext =>
        {
            foreach (var kvp in baseClassToMethods)
            {
                var methods = kvp.Value.ToList();
                if (methods.Count < 2)
                    continue;

                var duplicates = FindDuplicates(methods);
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

        static string GetBodyKey(MethodDeclarationSyntax m)
        {
            if (m.ExpressionBody is not null)
            {
                return m.ExpressionBody.Expression.NormalizeWhitespace().ToFullString();
            }
            if (m.Body is not null)
            {
                return m.Body.NormalizeWhitespace().ToFullString();
            }
            return string.Empty;
        }

        for (int i = 0; i < methods.Count; i++)
        {
            var key1 = GetBodyKey(methods[i]);
            for (int j = i + 1; j < methods.Count; j++)
            {
                var key2 = GetBodyKey(methods[j]);
                if (key1.Length > 0 && key1 == key2)
                {
                    duplicates.Add(methods[i]);
                    duplicates.Add(methods[j]);
                }
            }
        }

        return duplicates.Distinct();
    }
}