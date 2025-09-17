namespace Gendarme.Analyzers.Serialization;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ImplementISerializableCorrectlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ImplementISerializableCorrectlyTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ImplementISerializableCorrectlyMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ImplementISerializableCorrectlyDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ImplementISerializableCorrectly,
        Title,
        MessageFormat,
        Category.Serialization,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol namedType)
        {
            return;
        }

        var iSerializableType = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.ISerializable");
        if (iSerializableType is null)
        {
            return;
        }

        var implementsISerializable = namedType.AllInterfaces
            .Any(@interface => SymbolEqualityComparer.Default.Equals(@interface, iSerializableType));
        if (!implementsISerializable)
        {
            return;
        }

        var getObjectDataMethods = namedType.GetMembers("GetObjectData").OfType<IMethodSymbol>().ToList();
        if (getObjectDataMethods.Count == 0)
        {
            return;
        }

        foreach (var method in getObjectDataMethods)
        {
            if (namedType.IsSealed || method.IsVirtual)
            {
                continue;
            }

            var location = method.Locations.FirstOrDefault();
            if (location is null)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(Rule, location, namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
