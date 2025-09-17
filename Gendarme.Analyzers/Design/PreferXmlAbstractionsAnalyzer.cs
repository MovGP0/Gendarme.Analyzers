namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferXmlAbstractionsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.PreferXmlAbstractionsTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.PreferXmlAbstractionsMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.PreferXmlAbstractionsDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferXmlAbstractions,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // We'll check all named types for publicly visible members returning or taking an XmlDocument, XPathDocument, or XmlNode
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (!namedType.DeclaredAccessibility.HasFlag(Accessibility.Public) &&
            !namedType.DeclaredAccessibility.HasFlag(Accessibility.Protected))
        {
            return;
        }

        // Check methods and properties
        foreach (var member in namedType.GetMembers())
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) ||
                member.DeclaredAccessibility.HasFlag(Accessibility.Protected))
            {
                if (member is IMethodSymbol method)
                {
                    // Skip property accessors to avoid duplicate diagnostics (property itself is handled separately)
                    if (method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet)
                        continue;

                    // Check parameters
                    foreach (var param in method.Parameters)
                    {
                        CheckXmlType(param.Type, context, member);
                    }
                    // Check return type
                    if (!method.ReturnsVoid)
                    {
                        CheckXmlType(method.ReturnType, context, member);
                    }
                }
                else if (member is IPropertySymbol prop)
                {
                    CheckXmlType(prop.Type, context, member);
                }
            }
        }
    }

    private static void CheckXmlType(ITypeSymbol symbolType, SymbolAnalysisContext context, ISymbol locationSymbol)
    {
        // We look for classes like XmlDocument, XPathDocument, XmlNode
        string fullName = symbolType.ToDisplayString();

        if (fullName is "System.Xml.XmlDocument" or "System.Xml.XPath.XPathDocument" or "System.Xml.XmlNode" or "System.Xml.XmlElement")
        {
            var diag = Diagnostic.Create(
                Rule,
                locationSymbol.Locations.FirstOrDefault(),
                locationSymbol.Name,
                symbolType.Name);
            context.ReportDiagnostic(diag);
        }
    }
}