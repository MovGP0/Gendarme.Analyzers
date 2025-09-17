

// your namespace

namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConsiderConvertingMethodToPropertyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ConsiderConvertingMethodToPropertyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ConsiderConvertingMethodToPropertyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ConsiderConvertingMethodToPropertyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ConsiderConvertingMethodToProperty,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Skip constructors, operators, property accessors, etc.
        if (methodSymbol.MethodKind != MethodKind.Ordinary)
            return;

        // Check if the method name suggests 'GetFoo', 'IsBar', etc., has no parameters, is short, etc.
        if (methodSymbol.Parameters.Length == 0 &&
            (methodSymbol.Name.StartsWith("Get") || methodSymbol.Name.StartsWith("Is")) &&
            !methodSymbol.ReturnsVoid)
        {
            // Fire the diagnostic
            var location = methodSymbol.Locations.FirstOrDefault();

            var syntaxReference = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference is not null)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is MethodDeclarationSyntax methodDeclaration)
                {
                    location = methodDeclaration.Identifier.GetLocation();
                }
            }

            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                methodSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}