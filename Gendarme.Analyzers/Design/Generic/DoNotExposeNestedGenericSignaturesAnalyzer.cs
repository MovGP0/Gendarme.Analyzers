namespace Gendarme.Analyzers.Design.Generic;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DoNotExposeNestedGenericSignaturesAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId.DoNotExposeNestedGenericSignatures,
        title: Strings.DoNotExposeNestedGenericSignatures_Title,
        messageFormat: Strings.DoNotExposeNestedGenericSignatures_Message,
        description: Strings.DoNotExposeNestedGenericSignatures_Description,
        category: Category.Design,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        if (methodSymbol.DeclaredAccessibility == Accessibility.Public ||
            methodSymbol.DeclaredAccessibility == Accessibility.Protected)
        {
            CheckType(methodSymbol.ReturnType, methodSymbol, context);
            foreach (var parameter in methodSymbol.Parameters)
            {
                CheckType(parameter.Type, methodSymbol, context);
            }
        }
    }

    private static void CheckType(ITypeSymbol typeSymbol, IMethodSymbol methodSymbol, SymbolAnalysisContext context)
    {
        if (typeSymbol is not INamedTypeSymbol { IsGenericType: true } namedType)
        {
            return;
        }

        foreach (var argument in namedType.TypeArguments)
        {
            if (argument is INamedTypeSymbol { IsGenericType: true })
            {
                var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}