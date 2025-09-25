namespace Gendarme.Analyzers.Serialization;

/// <summary>
/// This rule checks for serializable types, i.e. decorated with the <c>[Serializable]</c> attribute,
/// and checks to see if all its fields are serializable as well.
/// If not the rule will fire unless the field is decorated with the <c>[NonSerialized]</c> attribute.
/// The rule will also warn if the field type is an interface as it is not possible, before execution time, to know for certain if the type can be serialized or not.
/// </summary>
/// <example>
/// Bad example:
/// <code language="csharp">
/// class NonSerializableClass {
/// }
/// 
/// [Serializable]
/// class SerializableClass {
///     NonSerializableClass field;
/// }
/// </code>
/// Good example:
/// <code language="csharp">
/// class NonSerializableClass {
/// }
/// 
/// [Serializable]
/// class SerializableClass {
///     [NonSerialized]
///     NonSerializableClass field;
/// }
/// </code>
/// </example>
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

        if (!namedType.GetAttributes().Any(attr => attr.AttributeClass != null && attr.AttributeClass.ToDisplayString() == "System.SerializableAttribute"))
            return;

        var fields = namedType.GetMembers().OfType<IFieldSymbol>()
            .Where(f => f is { IsStatic: false, IsImplicitlyDeclared: false, IsConst: false });

        foreach (var field in fields)
        {
            if (field.GetAttributes().Any(attr => attr.AttributeClass != null && attr.AttributeClass.ToDisplayString() == "System.NonSerializedAttribute"))
                continue;

            if (IsSerializable(field.Type))
                continue;

            if (field.Locations.Length > 0)
            {
                var diagnostic = Diagnostic.Create(Rule, field.Locations[0], field.Name, namedType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private bool IsSerializable(ITypeSymbol type)
    {
        // Interfaces are not considered serializable by this rule
        if (type.TypeKind == TypeKind.Interface)
            return false;

        // System.Object is too general to be safely serializable
        if (type.SpecialType == SpecialType.System_Object)
            return false;

        // Arrays are serializable if their element types are serializable
        if (type is IArrayTypeSymbol arrayType)
            return IsSerializable(arrayType.ElementType);

        // Nullable<T> is serializable if T is serializable
        if (type is INamedTypeSymbol named && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            return IsSerializable(named.TypeArguments[0]);

        // Enums are serializable
        if (type.TypeKind == TypeKind.Enum)
            return true;

        // Primitive and common types treated as serializable
        if (type.SpecialType is SpecialType.System_Boolean or SpecialType.System_Char or
            SpecialType.System_SByte or SpecialType.System_Byte or
            SpecialType.System_Int16 or SpecialType.System_UInt16 or
            SpecialType.System_Int32 or SpecialType.System_UInt32 or
            SpecialType.System_Int64 or SpecialType.System_UInt64 or
            SpecialType.System_Decimal or SpecialType.System_Double or SpecialType.System_Single or
            SpecialType.System_String)
            return true;

        // Check for the [Serializable] attribute on the type itself
        var serializableAttribute = type.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == "System.SerializableAttribute");

        return serializableAttribute;
    }

}