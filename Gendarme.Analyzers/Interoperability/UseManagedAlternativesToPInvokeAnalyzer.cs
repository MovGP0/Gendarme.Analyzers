using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Interoperability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseManagedAlternativesToPInvokeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseManagedAlternativesToPInvokeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseManagedAlternativesToPInvokeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseManagedAlternativesToPInvokeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseManagedAlternativesToPInvoke,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    // Map of P/Invoke methods to managed alternatives
    private static readonly ImmutableDictionary<string, string> PInvokeAlternatives = ImmutableDictionary.CreateRange([
        new KeyValuePair<string, string>("kernel32.dll|Sleep", "System.Threading.Thread.Sleep")
        // Add more mappings as needed
    ]);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        var methodSymbol = invocation.TargetMethod;

        // Check if the method is a P/Invoke method by verifying the DllImportAttribute
        var dllImportAttribute = methodSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.ToString() == "System.Runtime.InteropServices.DllImportAttribute");

        if (dllImportAttribute == null)
            return;

        var dllName = dllImportAttribute.ConstructorArguments[0].Value as string;
        var methodName = methodSymbol.Name;

        var key = $"{dllName}|{methodName}";
        if (PInvokeAlternatives.TryGetValue(key, out var managedAlternative))
        {
            var location = invocation.Syntax.GetLocation();
            var diagnostic = Diagnostic.Create(Rule, location, methodName, managedAlternative);
            context.ReportDiagnostic(diagnostic);
        }
    }
}