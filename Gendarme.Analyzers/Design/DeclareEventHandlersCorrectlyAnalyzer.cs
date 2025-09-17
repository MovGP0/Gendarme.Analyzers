namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DeclareEventHandlersCorrectlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DeclareEventHandlersCorrectlyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DeclareEventHandlersCorrectlyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DeclareEventHandlersCorrectlyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DeclareEventHandlersCorrectly,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // We'll check event symbols
        context.RegisterSymbolAction(AnalyzeEvent, SymbolKind.Event);
    }

    private static void AnalyzeEvent(SymbolAnalysisContext context)
    {
        var eventSymbol = (IEventSymbol)context.Symbol;

        // Check if the event's type matches EventHandler or EventHandler<T>,
        // or if the delegate has the standard (object, EventArgs) signature
        if (eventSymbol.Type is not INamedTypeSymbol delegateType)
            return;

        // If it's EventHandler or EventHandler<T>, it's presumably correct
        if (delegateType.Name == nameof(EventHandler) && delegateType.ContainingNamespace.Name == "System")
            return;

        // Otherwise, check the parameters
        var invokeMethod = delegateType.DelegateInvokeMethod;
        if (invokeMethod == null) return;

        // Must return void
        if (!invokeMethod.ReturnsVoid)
        {
            ReportDiagnostic(context, eventSymbol);
            return;
        }

        var parameters = invokeMethod.Parameters;
        if (parameters.Length != 2)
        {
            ReportDiagnostic(context, eventSymbol);
            return;
        }

        var firstParam = parameters[0];
        var secondParam = parameters[1];

        if (firstParam.Type.SpecialType != SpecialType.System_Object ||
            secondParam.Type.Name != nameof(EventArgs))
        {
            ReportDiagnostic(context, eventSymbol);
        }
    }

    private static void ReportDiagnostic(SymbolAnalysisContext context, IEventSymbol eventSymbol)
    {
        var diagnostic = Diagnostic.Create(
            Rule,
            eventSymbol.Locations[0],
            eventSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}