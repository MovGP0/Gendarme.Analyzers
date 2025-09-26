namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule looks for types that override <c>object.Equals(object)</c> but do not provide
/// a type-specific <c>Equals(T)</c> overload and an implementation of <c>IEquatable&lt;T&gt;</c>.
/// Such members avoid the boxing/casting associated with comparing through <c>object</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ImplementEqualsTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ImplementEqualsTypeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ImplementEqualsTypeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ImplementEqualsTypeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ImplementEqualsType,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private const string EquatableMetadataName = "System.IEquatable`1";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(static compilationStartContext =>
        {
            var equatableType = compilationStartContext.Compilation.GetTypeByMetadataName(EquatableMetadataName);

            compilationStartContext.RegisterSymbolAction(symbolContext =>
                AnalyzeNamedType(symbolContext, equatableType), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context, INamedTypeSymbol? equatableType)
    {
        var type = (INamedTypeSymbol)context.Symbol;

        if (type.IsImplicitlyDeclared || type.TypeKind is not (TypeKind.Class or TypeKind.Struct))
        {
            return;
        }

        if (!OverridesEquals(type))
        {
            return;
        }

        var hasTypeSpecificEquals = HasTypeSpecificEquals(type);
        var implementsEquatable = ImplementsEquatableInterface(type, equatableType);

        if (hasTypeSpecificEquals && implementsEquatable)
        {
            return;
        }

        var location = type.Locations.FirstOrDefault(static loc => loc.IsInSource);
        if (location is null)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name));
    }

    private static bool OverridesEquals(INamedTypeSymbol type) =>
        type.GetMembers(nameof(object.Equals))
            .OfType<IMethodSymbol>()
            .Any(static method => method is
            {
                IsOverride: true,
                IsStatic: false,
                Parameters.Length: 1,
                Parameters: [{ Type.SpecialType: SpecialType.System_Object }]
            });

    private static bool HasTypeSpecificEquals(INamedTypeSymbol type) =>
        type.GetMembers(nameof(object.Equals))
            .OfType<IMethodSymbol>()
            .Any(method => method is
            {
                IsStatic: false,
                Parameters.Length: 1,
                ReturnsVoid: false,
                ReturnType.SpecialType: SpecialType.System_Boolean
            } &&
            SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, type));

    private static bool ImplementsEquatableInterface(INamedTypeSymbol type, INamedTypeSymbol? equatableType)
    {
        if (equatableType is null)
        {
            return false;
        }

        foreach (var interfaceSymbol in type.AllInterfaces)
        {
            if (!SymbolEqualityComparer.Default.Equals(interfaceSymbol.OriginalDefinition, equatableType))
            {
                continue;
            }

            var typeArgument = interfaceSymbol.TypeArguments.Length == 1 ? interfaceSymbol.TypeArguments[0] : null;
            if (typeArgument is not null && SymbolEqualityComparer.Default.Equals(typeArgument, type))
            {
                return true;
            }
        }

        return false;
    }
}