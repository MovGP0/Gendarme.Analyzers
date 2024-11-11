using Gendarme.Analyzers.Extensions;

namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionShouldBeVisibleAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ExceptionShouldBeVisibleTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ExceptionShouldBeVisibleMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ExceptionShouldBeVisibleDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ExceptionShouldBeVisible,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (!namedTypeSymbol.InheritsFrom("System.Exception"))
            return;

        if (namedTypeSymbol.DeclaredAccessibility == Accessibility.Public)
        {
            return;
        }

        if (namedTypeSymbol.BaseType?.ToString()
            is "System.Exception"
            or "System.SystemException"
            or "System.ApplicationException")
        {
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}