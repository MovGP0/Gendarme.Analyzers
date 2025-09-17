namespace Gendarme.Analyzers.Security.Cas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotExposeFieldsInSecuredTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotExposeFieldsInSecuredTypeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotExposeFieldsInSecuredTypeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotExposeFieldsInSecuredTypeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotExposeFieldsInSecuredType,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly string SecurityActionTypeName = "System.Security.Permissions.SecurityAction";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

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

        // Check if the type has security demands
        var hasSecurityDemand = namedType.GetAttributes()
            .Any(attr => IsSecurityAttribute(attr) && IsDemandOrLinkDemand(attr));

        if (!hasSecurityDemand)
            return;

        // Find visible fields
        var publicFields = namedType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f.DeclaredAccessibility == Accessibility.Public);

        if (publicFields.Any())
        {
            var fieldNames = string.Join(", ", publicFields.Select(f => f.Name));
            var diagnostic = Diagnostic.Create(Rule, namedType.Locations[0], namedType.Name, fieldNames);
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

    private bool IsDemandOrLinkDemand(AttributeData attribute)
    {
        var action = GetSecurityAction(attribute);
        return action is SecurityAction.Demand or SecurityAction.LinkDemand;
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