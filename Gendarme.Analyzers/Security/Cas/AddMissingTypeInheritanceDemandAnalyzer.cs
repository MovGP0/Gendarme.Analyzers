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

    private const string CodeAccessSecurityAttributeName = "System.Security.Permissions.CodeAccessSecurityAttribute";
    private const string SecurityActionTypeName = "System.Security.Permissions.SecurityAction";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol namedType)
        {
            return;
        }

        if (namedType.TypeKind != TypeKind.Class || namedType.IsSealed)
        {
            return;
        }

        var codeAccessSecurityAttributeType = context.Compilation.GetTypeByMetadataName(CodeAccessSecurityAttributeName);
        var securityActionType = context.Compilation.GetTypeByMetadataName(SecurityActionTypeName);
        if (codeAccessSecurityAttributeType is null || securityActionType is null)
        {
            return;
        }

        var securityAttributes = namedType.GetAttributes()
            .Where(attribute => IsSecurityAttribute(attribute, codeAccessSecurityAttributeType));

        var hasLinkDemand = securityAttributes
            .Any(attribute => GetSecurityAction(attribute, securityActionType) == SecurityAction.LinkDemand);
        if (!hasLinkDemand)
        {
            return;
        }

        var hasInheritanceDemand = securityAttributes
            .Any(attribute => GetSecurityAction(attribute, securityActionType) == SecurityAction.InheritanceDemand);

        if (hasInheritanceDemand)
        {
            return;
        }

        var location = namedType.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, location, namedType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsSecurityAttribute(AttributeData attribute, INamedTypeSymbol codeAccessSecurityAttributeType)
    {
        var current = attribute.AttributeClass;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, codeAccessSecurityAttributeType))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static SecurityAction? GetSecurityAction(AttributeData attribute, INamedTypeSymbol securityActionType)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        var argument = attribute.ConstructorArguments[0];
        if (argument.Type is not null &&
            SymbolEqualityComparer.Default.Equals(argument.Type, securityActionType) &&
            argument.Value is int value)
        {
            return (SecurityAction)value;
        }

        return null;
    }
}
