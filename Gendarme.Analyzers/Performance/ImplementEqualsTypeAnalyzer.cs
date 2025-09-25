namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule looks for types that override <c>Object.Equals(object)</c>
/// but do not provide an <c>Equals(x)</c> overload using the type.
/// Such an overload removes the need to cast the object to the correct type.
/// For value types this also removes the costly boxing operations.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public class Bad {
///     public override bool Equals (object obj)
///     {
///         return base.Equals (obj);
///     }
///  
///     public override int GetHashCode ()
///     {
///         return base.GetHashCode ();
///     }
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// // IEquatable&lt;T> is only available since
/// // version 2.0 of the .NET framework
/// public class Good : IEquatable&lt;Good> {
///     public override bool Equals (object obj)
///     {
///         return (obj as Good);
///     }
///  
///     public bool Equals (Good other)
///     {
///         return (other != null);
///     }
///  
///     public override int GetHashCode ()
///     {
///         return base.GetHashCode ();
///     }
/// }
/// </code>
/// </example>
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

    private static readonly string IEquatableTypeName = "System.IEquatable`1";

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
        var type = (INamedTypeSymbol)context.Symbol;

        // Check if type overrides Equals(object)
        var overridesEquals = type.GetMembers("Equals")
            .OfType<IMethodSymbol>()
            .Any(m => m.Parameters is [{ Type.SpecialType: SpecialType.System_Object }] &&
                      m.IsOverride);

        if (!overridesEquals)
            return;

        // Check if type implements Equals(T)
        var hasTypeSpecificEquals = type.GetMembers("Equals")
            .OfType<IMethodSymbol>()
            .Any(m => m.Parameters.Length == 1 &&
                      SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, type) &&
                      !m.IsStatic);

        // Check if type implements IEquatable<T>
        var implementsIEquatable = type.AllInterfaces
            .Any(i => i.OriginalDefinition.ToDisplayString() == IEquatableTypeName &&
                      SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], type));

        if (!hasTypeSpecificEquals || !implementsIEquatable)
        {
            var diagnostic = Diagnostic.Create(Rule, type.Locations[0], type.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}