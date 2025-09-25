namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule will fire if a type is only visible within its assembly, can be instantiated, but is not instantiated.
/// Such types are often leftover (dead code) or are debugging/testing code and not required.
/// However, in some case the types might be needed, e.g. when accessed through reflection
/// or if the <c>[InternalsVisibleTo]</c> attribute is used on the assembly.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// // defined, but never instantiated
/// internal class MyInternalClass {
///     // ...
/// }
///  
/// public class MyClass {
///     static void Main ()
///     {
///         // ...
///     }
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// internal class MyInternalClass {
///     // ...
/// }
///  
/// public class MyClass {
///     static void Main ()
///     {
///         MyInternalClass c = new MyInternalClass ();
///         // ...
///     }
/// }
/// </code>
/// </example>
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
        description: Description,
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

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

            if (operation is IObjectCreationOperation { Type: { } objectCreationType })
            {
                instantiatedTypes.Add(objectCreationType);
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