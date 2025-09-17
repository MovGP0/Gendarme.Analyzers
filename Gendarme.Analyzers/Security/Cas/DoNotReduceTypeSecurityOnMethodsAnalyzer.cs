namespace Gendarme.Analyzers.Security.Cas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotReduceTypeSecurityOnMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotReduceTypeSecurityOnMethodsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotReduceTypeSecurityOnMethodsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotReduceTypeSecurityOnMethodsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotReduceTypeSecurityOnMethods,
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
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol method || method.ContainingType is null)
        {
            return;
        }

        var codeAccessSecurityAttributeType = context.Compilation.GetTypeByMetadataName(CodeAccessSecurityAttributeName);
        var securityActionType = context.Compilation.GetTypeByMetadataName(SecurityActionTypeName);
        if (codeAccessSecurityAttributeType is null || securityActionType is null)
        {
            return;
        }

        var typeSecurityActions = GetSecurityActions(method.ContainingType.GetAttributes(), codeAccessSecurityAttributeType, securityActionType);
        var methodSecurityActions = GetSecurityActions(method.GetAttributes(), codeAccessSecurityAttributeType, securityActionType);

        if (methodSecurityActions.Count == 0 || IsSubset(methodSecurityActions, typeSecurityActions))
        {
            return;
        }

        var location = method.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, location, method.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static ImmutableHashSet<SecurityAction> GetSecurityActions(
        ImmutableArray<AttributeData> attributes,
        INamedTypeSymbol codeAccessSecurityAttributeType,
        INamedTypeSymbol securityActionType)
    {
        var builder = ImmutableHashSet.CreateBuilder<SecurityAction>();

        foreach (var attribute in attributes)
        {
            if (!IsSecurityAttribute(attribute, codeAccessSecurityAttributeType))
            {
                continue;
            }

            var securityAction = GetSecurityAction(attribute, securityActionType);
            if (securityAction.HasValue)
            {
                builder.Add(securityAction.Value);
            }
        }

        return builder.ToImmutable();
    }

    private static bool IsSubset(ImmutableHashSet<SecurityAction> subset, ImmutableHashSet<SecurityAction> superset)
    {
        return subset.All(superset.Contains);
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

