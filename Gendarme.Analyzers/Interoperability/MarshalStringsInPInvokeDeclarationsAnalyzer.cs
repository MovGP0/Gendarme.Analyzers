namespace Gendarme.Analyzers.Interoperability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarshalStringsInPInvokeDeclarationsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MarshalStringsInPInvokeDeclarationsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MarshalStringsInPInvokeDeclarationsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MarshalStringsInPInvokeDeclarationsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MarshalStringsInPInvokeDeclarations,
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
        var dllImportAttribute = methodSymbol.GetAttributes().FirstOrDefault(attr => attr.AttributeClass.ToString() == "System.Runtime.InteropServices.DllImportAttribute");
        if (dllImportAttribute == null)
            return;

        var hasCharSet = dllImportAttribute.NamedArguments.Any(arg => arg.Key == "CharSet");
        var hasStringParameters = methodSymbol.Parameters.Any(p => IsStringType(p.Type));

        if (!hasCharSet && hasStringParameters)
        {
            foreach (var parameter in methodSymbol.Parameters)
            {
                if (IsStringType(parameter.Type))
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
        }
    }

    private static bool IsStringType(ITypeSymbol type)
    {
        return type.SpecialType == SpecialType.System_String || type.ToString() == "System.Text.StringBuilder";
    }
}