namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidVisibleNestedTypesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidVisibleNestedTypesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidVisibleNestedTypesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidVisibleNestedTypesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidVisibleNestedTypes,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // If the type is nested and is externally visible
        if (namedTypeSymbol.ContainingType != null && namedTypeSymbol.IsExternallyVisible())
        {
            var location = namedTypeSymbol.Locations.FirstOrDefault();

            var syntaxReference = namedTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference is not null)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is TypeDeclarationSyntax typeDeclaration)
                {
                    location = typeDeclaration.Identifier.GetLocation();
                }
            }

            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}