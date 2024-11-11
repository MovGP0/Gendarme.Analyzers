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

    private static readonly string SecurityActionTypeName = "System.Security.Permissions.SecurityAction";

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

        var containingType = method.ContainingType;

        // Get type's security actions
        var typeSecurityActions = GetSecurityActions(containingType.GetAttributes());

        // Get method's security actions
        var methodSecurityActions = GetSecurityActions(method.GetAttributes());

        // If method's security actions are not a subset of type's, report diagnostic
        if (!IsSubset(methodSecurityActions, typeSecurityActions))
        {
            var diagnostic = Diagnostic.Create(Rule, method.Locations[0], method.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private ImmutableHashSet<SecurityAction> GetSecurityActions(ImmutableArray<AttributeData> attributes)
    {
        return attributes
            .Where(attr => IsSecurityAttribute(attr))
            .Select(attr => GetSecurityAction(attr))
            .Where(sa => sa.HasValue)
            .Select(sa => sa.Value)
            .ToImmutableHashSet();
    }

    private bool IsSubset(ImmutableHashSet<SecurityAction> subset, ImmutableHashSet<SecurityAction> superset)
    {
        return subset.All(sa => superset.Contains(sa));
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