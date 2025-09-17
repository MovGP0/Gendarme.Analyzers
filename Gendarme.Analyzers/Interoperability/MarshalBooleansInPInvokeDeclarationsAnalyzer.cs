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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        var dllImportAttributeType = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.DllImportAttribute");
        var marshalAsAttributeType = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.MarshalAsAttribute");
        if (dllImportAttributeType is null)
        {
            return;
        }

        var hasDllImport = methodSymbol.GetAttributes()
            .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, dllImportAttributeType));
        if (!hasDllImport)
        {
            return;
        }

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.Type?.SpecialType != SpecialType.System_Boolean)
            {
                continue;
            }

            var hasMarshalAs = parameter.GetAttributes().Any(attribute =>
                marshalAsAttributeType is not null
                    ? SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marshalAsAttributeType)
                    : attribute.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.MarshalAsAttribute");

            if (hasMarshalAs)
            {
                continue;
            }

            var location = parameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken)?.GetLocation();
            if (location is null)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, location, parameter.Name);
            context.ReportDiagnostic(diagnostic);
        }

        if (methodSymbol.ReturnType?.SpecialType != SpecialType.System_Boolean)
        {
            return;
        }

        var returnHasMarshalAs = methodSymbol.GetReturnTypeAttributes().Any(attribute =>
            marshalAsAttributeType is not null
                ? SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, marshalAsAttributeType)
                : attribute.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.MarshalAsAttribute");

        if (returnHasMarshalAs)
        {
            return;
        }

        var methodLocation = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken)?.GetLocation();
        if (methodLocation is null)
        {
            return;
        }

        var returnDiagnostic = Diagnostic.Create(Rule, methodLocation, "return value");
        context.ReportDiagnostic(returnDiagnostic);
    }
}
