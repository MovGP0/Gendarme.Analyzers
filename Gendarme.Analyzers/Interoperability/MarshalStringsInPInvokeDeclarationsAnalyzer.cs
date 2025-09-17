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
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        var dllImportAttributeType = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.DllImportAttribute");
        if (dllImportAttributeType is null)
        {
            return;
        }

        var marshalAsAttributeType = context.Compilation.GetTypeByMetadataName("System.Runtime.InteropServices.MarshalAsAttribute");
        var stringBuilderType = context.Compilation.GetTypeByMetadataName("System.Text.StringBuilder");

        var dllImportAttribute = methodSymbol.GetAttributes()
            .FirstOrDefault(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, dllImportAttributeType));
        if (dllImportAttribute is null)
        {
            return;
        }

        var hasCharSet = dllImportAttribute.NamedArguments.Any(arg => arg.Key == "CharSet");
        var hasStringParameters = methodSymbol.Parameters
            .Any(parameter => IsStringType(parameter.Type, stringBuilderType));

        if (hasCharSet || !hasStringParameters)
        {
            return;
        }

        foreach (var parameter in methodSymbol.Parameters)
        {
            if (!IsStringType(parameter.Type, stringBuilderType))
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
            if (location is not null)
            {
                var diagnostic = Diagnostic.Create(Rule, location, parameter.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsStringType(ITypeSymbol? type, INamedTypeSymbol? stringBuilderType)
    {
        return type is not null &&
               (type.SpecialType == SpecialType.System_String ||
                (stringBuilderType is not null && SymbolEqualityComparer.Default.Equals(type, stringBuilderType)) ||
                type.ToDisplayString() == "System.Text.StringBuilder");
    }
}
