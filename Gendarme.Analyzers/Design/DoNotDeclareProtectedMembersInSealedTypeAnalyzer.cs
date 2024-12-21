namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotDeclareProtectedMembersInSealedTypeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.DoNotDeclareProtectedMembersInSealedTypeTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.DoNotDeclareProtectedMembersInSealedTypeMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.DoNotDeclareProtectedMembersInSealedTypeDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotDeclareProtectedMembersInSealedType,
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
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (!namedType.IsSealed) return;

        // Check each member for 'protected' accessibility
        foreach (var member in namedType.GetMembers())
        {
            if (member.DeclaredAccessibility == Accessibility.Protected ||
                member.DeclaredAccessibility == Accessibility.ProtectedOrInternal ||
                member.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    member.Locations[0],
                    namedType.Name,
                    member.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}