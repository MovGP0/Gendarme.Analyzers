namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidConstructorsInStaticTypesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidConstructorsInStaticTypes_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidConstructorsInStaticTypes_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidConstructorsInStaticTypes_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidConstructorsInStaticTypes,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol { TypeKind: TypeKind.Class } namedType)
        {
            return;
        }

        if (!IsStaticLikeType(namedType))
        {
            return;
        }

        foreach (var constructor in namedType.InstanceConstructors)
        {
            if (constructor.IsImplicitlyDeclared)
            {
                continue;
            }

            if (IsVisibleConstructor(constructor))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, constructor.Locations[0], namedType.Name));
            }
        }
    }

    private static bool IsStaticLikeType(INamedTypeSymbol type)
    {
        if (type.IsStatic)
        {
            return true;
        }

        foreach (var member in type.GetMembers())
        {
            if (!ShouldConsiderMember(member))
            {
                continue;
            }

            if (!member.IsStatic)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ShouldConsiderMember(ISymbol member)
    {
        if (member.IsImplicitlyDeclared)
        {
            return false;
        }

        return member switch
        {
            IMethodSymbol method => method.MethodKind is MethodKind.Ordinary or MethodKind.UserDefinedOperator or MethodKind.Conversion,
            IPropertySymbol => true,
            IFieldSymbol => true,
            IEventSymbol => true,
            _ => false,
        };
    }

    private static bool IsVisibleConstructor(IMethodSymbol constructor)
    {
        return constructor.DeclaredAccessibility
            is Accessibility.Public
            or Accessibility.Protected
            or Accessibility.ProtectedOrInternal
            or Accessibility.ProtectedAndInternal;
    }
}
