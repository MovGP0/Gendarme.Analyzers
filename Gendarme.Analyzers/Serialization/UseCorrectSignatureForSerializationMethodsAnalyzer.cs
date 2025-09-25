namespace Gendarme.Analyzers.Serialization;

/// <summary>
/// This rule checks for methods which use the serialization attributes:
/// <c>[OnSerializing, OnDeserializing, OnSerialized, OnDeserialized]</c>.
/// You must ensure that these methods have the correct signature.
/// They should be <c>private</c>, return <c>void</c> and have a single parameter of type <c>StreamingContext</c>.
/// Failure to have the right signature can, in some circumstances, make your assembly unusable at runtime.
/// </summary>
/// <example>
/// Bad example:
/// <code language="c#">
/// [Serializable]
/// public class Bad {
///     [OnSerializing]
///     public bool Serializing (StreamingContext context)
///     {
///     }
/// }
/// </code>
/// Good example:
/// <code language="c#">
/// [Serializable]
/// public class BadClass {
///     [OnSerializing]
///     private void Serializing (StreamingContext context)
///     {
///     }
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCorrectSignatureForSerializationMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseCorrectSignatureForSerializationMethodsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseCorrectSignatureForSerializationMethodsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseCorrectSignatureForSerializationMethodsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseCorrectSignatureForSerializationMethods,
        Title,
        MessageFormat,
        Category.Serialization,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableHashSet<string> SerializationAttributes = ImmutableHashSet.Create(
        StringComparer.Ordinal,
        "System.Runtime.Serialization.OnSerializingAttribute",
        "System.Runtime.Serialization.OnSerializedAttribute",
        "System.Runtime.Serialization.OnDeserializingAttribute",
        "System.Runtime.Serialization.OnDeserializedAttribute");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IMethodSymbol method)
        {
            return;
        }

        var hasSerializationAttribute = method.GetAttributes()
            .Any(attribute => attribute.AttributeClass is not null &&
                               SerializationAttributes.Contains(attribute.AttributeClass.ToDisplayString()));

        if (!hasSerializationAttribute)
        {
            return;
        }

        var streamingContextType = context.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.StreamingContext");
        var parameterType = method.Parameters.Length == 1 ? method.Parameters[0].Type : null;

        var isValidSignature = method.DeclaredAccessibility == Accessibility.Private &&
                               method.ReturnType.SpecialType == SpecialType.System_Void &&
                               method.Parameters.Length == 1 &&
                               parameterType is not null &&
                               (streamingContextType is null || SymbolEqualityComparer.Default.Equals(parameterType, streamingContextType));

        if (isValidSignature)
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
}
