namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule looks for InitOnly fields (readonly in C#) that could be turned into Literal (const in C#)
/// because their value is known at compile time.
/// Literal fields don’t need to be initialized
/// (i.e. they don’t force the compiler to add a static constructor to the type)
/// resulting in less code and the value (not a reference to the field)
/// will be directly used in the IL (which is OK if the field has internal visibility,
/// but is often problematic if the field is visible outside the assembly).
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public class ClassWithReadOnly {
///     static readonly int One = 1;
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public class ClassWithConst
/// {
///     const int One = 1;
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferLiteralOverInitOnlyFieldsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferLiteralOverInitOnlyFields,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze field symbols
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
    }

    private void AnalyzeFieldSymbol(SymbolAnalysisContext context)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;

        // Check if field is static readonly
        if (!fieldSymbol.IsStatic || !fieldSymbol.IsReadOnly)
            return;

        // Check if field is initialized with a compile-time constant
        if (fieldSymbol.HasConstantValue)
        {
            var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}