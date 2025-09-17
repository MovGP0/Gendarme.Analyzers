namespace Gendarme.Analyzers.Security.Cas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SecureGetObjectDataOverridesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.SecureGetObjectDataOverridesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.SecureGetObjectDataOverridesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.SecureGetObjectDataOverridesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.SecureGetObjectDataOverrides,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const string ISerializableTypeName = "System.Runtime.Serialization.ISerializable";
    private const string SecurityPermissionAttributeName = "System.Security.Permissions.SecurityPermissionAttribute";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol method || method.ContainingType is null)
        {
            return;
        }

        if (!string.Equals(method.Name, "GetObjectData", StringComparison.Ordinal) || method.Parameters.Length != 2)
        {
            return;
        }

        var iSerializableType = context.Compilation.GetTypeByMetadataName(ISerializableTypeName);
        var securityPermissionAttributeType = context.Compilation.GetTypeByMetadataName(SecurityPermissionAttributeName);
        if (iSerializableType is null || securityPermissionAttributeType is null)
        {
            return;
        }

        if (!method.ContainingType.AllInterfaces.Any(@interface => SymbolEqualityComparer.Default.Equals(@interface, iSerializableType)))
        {
            return;
        }

        var hasRequiredSecurityAttribute = method.GetAttributes().Any(attribute =>
            SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, securityPermissionAttributeType) &&
            HasSerializationFormatterPermission(attribute));

        if (hasRequiredSecurityAttribute)
        {
            return;
        }

        var location = method.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, location, method.ContainingType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool HasSerializationFormatterPermission(AttributeData attribute)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (argument is { Key: "SerializationFormatter", Value.Value: true })
            {
                return true;
            }
        }

        return false;
    }
}
