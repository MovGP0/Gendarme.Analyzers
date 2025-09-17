namespace Gendarme.Analyzers.Security.Cas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReviewSealedTypeWithInheritanceDemandAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ReviewSealedTypeWithInheritanceDemandTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ReviewSealedTypeWithInheritanceDemandMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ReviewSealedTypeWithInheritanceDemandDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ReviewSealedTypeWithInheritanceDemand,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Info,
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

        // Only consider sealed classes
        if (!namedType.IsSealed || namedType.TypeKind != TypeKind.Class)
            return;

        // Check if it has an InheritanceDemand
        var hasInheritanceDemand = namedType.GetAttributes()
            .Any(attr => IsSecurityAttribute(attr) && GetSecurityAction(attr) == SecurityAction.InheritanceDemand);

        if (hasInheritanceDemand)
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