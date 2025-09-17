namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnusedPrivateFieldsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnusedPrivateFieldsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnusedPrivateFieldsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnusedPrivateFieldsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnusedPrivateFields,
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
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        var privateFields = namedType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f is { DeclaredAccessibility: Accessibility.Private, IsImplicitlyDeclared: false });

        var usedFields = new HashSet<IFieldSymbol>();

        foreach (var member in namedType.GetMembers().OfType<IMethodSymbol>())
        {
            if (member.DeclaringSyntaxReferences.Length == 0)
                continue;

            foreach (var syntaxRef in member.DeclaringSyntaxReferences)
            {
                var syntax = syntaxRef.GetSyntax(context.CancellationToken);
                var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);

                var dataFlowAnalysis = semanticModel.AnalyzeDataFlow(syntax);

                usedFields.UnionWith(dataFlowAnalysis.ReadInside.OfType<IFieldSymbol>());
                usedFields.UnionWith(dataFlowAnalysis.WrittenInside.OfType<IFieldSymbol>());
            }
        }

        foreach (var field in privateFields)
        {
            if (!usedFields.Contains(field))
            {
                var diagnostic = Diagnostic.Create(Rule, field.Locations[0], field.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}