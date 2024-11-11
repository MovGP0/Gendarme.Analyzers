namespace Gendarme.Analyzers.Security.Cas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AddMissingTypeInheritanceDemandAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AddMissingTypeInheritanceDemandTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AddMissingTypeInheritanceDemandMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AddMissingTypeInheritanceDemandDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AddMissingTypeInheritanceDemand,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly string SecurityActionTypeName = "System.Security.Permissions.SecurityAction";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        // Only consider classes
        if (namedType.TypeKind != TypeKind.Class)
            return;

        // Skip sealed types
        if (namedType.IsSealed)
            return;

        // Get security attributes
        var securityAttributes = namedType.GetAttributes()
            .Where(attr => IsSecurityAttribute(attr));

        // Check if it has a LinkDemand
        var hasLinkDemand = securityAttributes.Any(attr => GetSecurityAction(attr) == SecurityAction.LinkDemand);

        if (!hasLinkDemand)
            return;

        // Check if it has an InheritanceDemand
        var hasInheritanceDemand = securityAttributes.Any(attr => GetSecurityAction(attr) == SecurityAction.InheritanceDemand);

        if (!hasInheritanceDemand)
        {
            var diagnostic = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsSecurityAttribute(AttributeData attribute)
    {
        var baseType = attribute.AttributeClass;
        while (baseType != null)
        {
            if (baseType.ToDisplayString() == "System.Security.Permissions.CodeAccessSecurityAttribute")
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private SecurityAction? GetSecurityAction(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 0)
        {
            var arg = attribute.ConstructorArguments[0];
            if (arg.Type.Name == "SecurityAction" && arg.Value != null)
            {
                return (SecurityAction)(int)arg.Value;
            }
        }
        return null;
    }
}