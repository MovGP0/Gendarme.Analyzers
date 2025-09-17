

// your namespace

namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConsiderConvertingFieldToNullableAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.ConsiderConvertingFieldToNullableTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.ConsiderConvertingFieldToNullableMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.ConsiderConvertingFieldToNullableDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ConsiderConvertingFieldToNullable,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // A naive approach: analyze named types, look for pairs of fields "hasXYZ" (bool) and "xyz" (int, etc.)
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        var fields = namedType.GetMembers().OfType<IFieldSymbol>().ToList();

        // Very naive approach: if there's a bool field named "hasX" and an int field named "x", we flag it
        for (var i = 0; i < fields.Count; i++)
        {
            var fi = fields[i];
            if (fi.Type.SpecialType == SpecialType.System_Boolean && fi.Name.StartsWith("has"))
            {
                var suffix = fi.Name.Substring("has".Length); // e.g. "Foo" from "hasFoo"

                // Check if there's a field with the same suffix
                var matchingField = fields.FirstOrDefault(
                    f => f.Name.Equals(suffix, StringComparison.OrdinalIgnoreCase) &&
                         f.Type.IsValueType &&
                         f.Type.SpecialType != SpecialType.System_Boolean);

                if (matchingField != null)
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        fi.Locations[0],
                        namedType.Name // {0} in the message format
                    );
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}