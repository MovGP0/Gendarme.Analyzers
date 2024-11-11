namespace Gendarme.Analyzers.Interoperability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarshalBooleansInPInvokeDeclarationsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MarshalBooleansInPInvokeDeclarationsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MarshalBooleansInPInvokeDeclarationsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MarshalBooleansInPInvokeDeclarationsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MarshalBooleansInPInvokeDeclarations,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Check if the method is a P/Invoke method by verifying the DllImportAttribute
        var hasDllImport = methodSymbol.GetAttributes().Any(attr => attr.AttributeClass.ToString() == "System.Runtime.InteropServices.DllImportAttribute");
        if (!hasDllImport)
            return;

        // Check parameters for boolean types without MarshalAs attribute
        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Type.SpecialType == SpecialType.System_Boolean)
            {
                var hasMarshalAs = parameter.GetAttributes().Any(attr => attr.AttributeClass.ToString() == "System.Runtime.InteropServices.MarshalAsAttribute");
                if (!hasMarshalAs)
                {
                    var location = parameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation();
                    if (location != null)
                    {
                        var diagnostic = Diagnostic.Create(Rule, location, parameter.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        // Check return type if it's a boolean without MarshalAs attribute
        if (methodSymbol.ReturnType.SpecialType == SpecialType.System_Boolean)
        {
            var hasMarshalAs = methodSymbol.GetReturnTypeAttributes().Any(attr => attr.AttributeClass.ToString() == "System.Runtime.InteropServices.MarshalAsAttribute");
            if (!hasMarshalAs)
            {
                var location = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation();
                if (location != null)
                {
                    var diagnostic = Diagnostic.Create(Rule, location, "return value");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}