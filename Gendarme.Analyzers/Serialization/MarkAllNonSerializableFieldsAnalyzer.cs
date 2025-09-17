namespace Gendarme.Analyzers.Serialization;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarkAllNonSerializableFieldsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MarkAllNonSerializableFieldsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MarkAllNonSerializableFieldsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MarkAllNonSerializableFieldsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MarkAllNonSerializableFields,
        Title,
        MessageFormat,
        Category.Serialization,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        if (!namedType.GetAttributes().Any(attr => attr.AttributeClass.ToDisplayString() == "System.SerializableAttribute"))
            return;

        var fields = namedType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f is { IsStatic: false, IsImplicitlyDeclared: false, IsConst: false });

        foreach (var field in fields)
        {
            if (field.GetAttributes().Any(attr => attr.AttributeClass.ToDisplayString() == "System.NonSerializedAttribute"))
                continue;

            if (IsSerializable(field.Type))
                continue;

            var diagnostic = Diagnostic.Create(Rule, field.Locations[0], field.Name, namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsSerializable(ITypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Interface)
            return false;

        // Check for the [Serializable] attribute
        var serializableAttribute = type.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == "System.SerializableAttribute");

        return serializableAttribute;
    }

}