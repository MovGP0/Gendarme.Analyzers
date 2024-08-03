using System.Runtime.CompilerServices;

namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseMethodImplOptionsSynchronizedAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = Strings.DoNotUseMethodImplOptionsSynchronizedAnalyzer_Title;
    private static readonly LocalizableString MessageFormat = Strings.DoNotUseMethodImplOptionsSynchronizedAnalyzer_Message;
    private static readonly LocalizableString Description = Strings.DoNotUseMethodImplOptionsSynchronizedAnalyzer_Description;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotUseMethodImplOptionsSynchronized,
        Title,
        MessageFormat,
        Category.Concurrency,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;
        var attributes = methodSymbol.GetAttributes();

        foreach (var attribute in attributes)
        {
            if (attribute.AttributeClass?.ToString() == "System.Runtime.CompilerServices.MethodImplAttribute" &&
                attribute.ConstructorArguments.Any(arg => arg.Value is (int)MethodImplOptions.Synchronized))
            {
                var diagnostic = Diagnostic.Create(Rule, methodSymbol.Locations[0], methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}