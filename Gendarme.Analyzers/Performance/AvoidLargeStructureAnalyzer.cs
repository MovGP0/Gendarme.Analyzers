namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidLargeStructureAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidLargeStructureTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidLargeStructureMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidLargeStructureDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidLargeStructure,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int DefaultMaxSizeInBytes = 16;

    /// <summary>
    /// Configuration property to allow customization of max size
    /// </summary>
    private int MaxSizeInBytes { get; set; } = DefaultMaxSizeInBytes;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze named types (structs)
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var structSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze structs
        if (structSymbol.TypeKind != TypeKind.Struct)
            return;

        int sizeInBytes = 0;
        foreach (var field in structSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            sizeInBytes += GetSizeOfType(field.Type);
        }

        if (sizeInBytes > MaxSizeInBytes)
        {
            var diagnostic = Diagnostic.Create(Rule, structSymbol.Locations[0], structSymbol.Name, MaxSizeInBytes);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private int GetSizeOfType(ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
                return 1;
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_Char:
                return 2;
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Single:
                return 4;
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Double:
                return 8;
            case SpecialType.System_Decimal:
                return 16;
            default:
                if (type.TypeKind == TypeKind.Struct)
                {
                    int size = 0;
                    foreach (var field in type.GetMembers().OfType<IFieldSymbol>())
                    {
                        size += GetSizeOfType(field.Type);
                    }
                    return size;
                }
                else
                {
                    // Reference types are pointer-sized
                    return 8;
                }
        }
    }
}