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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze methods
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;

        if (method.Name != "GetObjectData")
            return;

        if (method.Parameters.Length != 2)
            return;

        if (!method.ContainingType.AllInterfaces.Any(i => i.ToDisplayString() == ISerializableTypeName))
            return;

        // Check if method is protected with SerializationFormatter permission
        var hasSecurityAttribute = method.GetAttributes()
            .Any(attr => IsSecurityAttribute(attr) && HasSerializationFormatterPermission(attr));

        if (!hasSecurityAttribute)
        {
            var diagnostic = Diagnostic.Create(Rule, method.Locations[0], method.ContainingType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsSecurityAttribute(AttributeData attribute)
    {
        var attrName = attribute.AttributeClass.ToDisplayString();
        return attrName == "System.Security.Permissions.SecurityPermissionAttribute";
    }

    private bool HasSerializationFormatterPermission(AttributeData attribute)
    {
        var namedArgs = attribute.NamedArguments;
        foreach (var arg in namedArgs)
        {
            if (arg.Key == "SerializationFormatter" && arg.Value.Value is bool b && b)
                return true;
        }
        return false;
    }
}