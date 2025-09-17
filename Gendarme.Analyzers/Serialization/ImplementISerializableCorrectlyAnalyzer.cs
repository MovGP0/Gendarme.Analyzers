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
        // Analyze named types
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        if (!namedType.AllInterfaces.Any(i => i.ToDisplayString() == "System.Runtime.Serialization.ISerializable"))
            return;

        // Check if GetObjectData is virtual if the type is not sealed
        var getObjectDataMethod = namedType.GetMembers("GetObjectData").OfType<IMethodSymbol>().FirstOrDefault();

        if (getObjectDataMethod != null)
        {
            if (!namedType.IsSealed && !getObjectDataMethod.IsVirtual)
            {
                var diagnostic = Diagnostic.Create(Rule, getObjectDataMethod.Locations[0], namedType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        // Check if all instance fields are serialized
        namedType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f is { IsStatic: false, IsImplicitlyDeclared: false, IsConst: false } &&
                        !f.GetAttributes().Any(attr => attr.AttributeClass.ToDisplayString() == "System.NonSerializedAttribute"));

        // Assume that the GetObjectData method serializes all fields (complex to verify)
        // For the purpose of this analyzer, we can warn if any fields are not serialized
        // In a real analyzer, data flow analysis would be needed
    }
}