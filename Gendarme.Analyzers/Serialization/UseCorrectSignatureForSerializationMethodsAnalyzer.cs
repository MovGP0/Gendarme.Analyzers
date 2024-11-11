namespace Gendarme.Analyzers.Serialization;

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
        "System.Runtime.Serialization.OnSerializingAttribute",
        "System.Runtime.Serialization.OnSerializedAttribute",
        "System.Runtime.Serialization.OnDeserializingAttribute",
        "System.Runtime.Serialization.OnDeserializedAttribute"
    );

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

        var hasSerializationAttribute = method.GetAttributes()
            .Any(attr => SerializationAttributes.Contains(attr.AttributeClass.ToDisplayString()));

        if (!hasSerializationAttribute)
            return;

        if (method.DeclaredAccessibility != Accessibility.Private ||
            method.ReturnType.SpecialType != SpecialType.System_Void ||
            method.Parameters.Length != 1 ||
            method.Parameters[0].Type.ToDisplayString() != "System.Runtime.Serialization.StreamingContext")
        {
            var diagnostic = Diagnostic.Create(Rule, method.Locations[0], method.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}