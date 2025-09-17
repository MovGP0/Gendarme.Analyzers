namespace Gendarme.Analyzers.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidAlwaysNullFieldAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidAlwaysNullFieldTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidAlwaysNullFieldMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidAlwaysNullFieldDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidAlwaysNullField,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
    }

    private static void AnalyzeFieldSymbol(SymbolAnalysisContext context)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;

        // Check if the field is private
        if (fieldSymbol.DeclaredAccessibility != Accessibility.Private)
            return;

        // Check if the field is a reference type
        if (fieldSymbol.Type.IsValueType)
            return;

        // Check if the field is assigned anywhere
        var references = context.Symbol.DeclaringSyntaxReferences;
        var isAssigned = false;

        foreach (var reference in references)
        {
            var syntax = reference.GetSyntax(context.CancellationToken) as VariableDeclaratorSyntax;
            if (syntax?.Initializer != null)
            {
                isAssigned = true;
                break;
            }

            var typeDeclaration = syntax?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (typeDeclaration == null)
                continue;

            var semanticModel = context.Compilation.GetSemanticModel(typeDeclaration.SyntaxTree);
            var assignments = typeDeclaration.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>()
                .Where(a => SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(a.Left).Symbol, fieldSymbol));

            if (assignments.Any())
            {
                isAssigned = true;
                break;
            }
        }

        if (!isAssigned)
        {
            var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
