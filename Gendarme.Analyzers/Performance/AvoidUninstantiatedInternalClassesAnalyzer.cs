using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUninstantiatedInternalClassesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUninstantiatedInternalClassesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUninstantiatedInternalClassesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUninstantiatedInternalClassesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUninstantiatedInternalClasses,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private void AnalyzeCompilationStart(CompilationStartAnalysisContext context)
    {
        var instantiatedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        context.RegisterOperationAction(operationContext =>
        {
            var operation = operationContext.Operation;

            if (operation is IObjectCreationOperation objectCreation)
            {
                instantiatedTypes.Add(objectCreation.Type);
            }
        }, OperationKind.ObjectCreation);

        context.RegisterCompilationEndAction(compilationContext =>
        {
            var compilation = compilationContext.Compilation;
            var allTypes = GetAllTypes(compilation.GlobalNamespace)
                .Where(t => t is { DeclaredAccessibility: Accessibility.Internal, TypeKind: TypeKind.Class, IsAbstract: false });

            foreach (var type in allTypes)
            {
                if (!instantiatedTypes.Contains(type))
                {
                    var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.Name);
                    compilationContext.ReportDiagnostic(diagnostic);
                }
            }
        });
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol namespaceSymbol)
    {
        foreach (var member in namespaceSymbol.GetMembers())
        {
            switch (member)
            {
                case INamespaceSymbol nestedNamespace:
                {
                    foreach (var type in GetAllTypes(nestedNamespace))
                        yield return type;
                    break;
                }
                case INamedTypeSymbol namedType:
                    yield return namedType;
                    break;
            }
        }
    }
}