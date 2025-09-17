using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumsShouldDefineAZeroValueAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.EnumsShouldDefineAZeroValueTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.EnumsShouldDefineAZeroValueMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.EnumsShouldDefineAZeroValueDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.EnumsShouldDefineAZeroValue,
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
        if (namedType.TypeKind != TypeKind.Enum)
            return;

        // Check if it's flagged
        var isFlags = namedType.GetAttributes().Any(a => a.AttributeClass?.Name == nameof(System.FlagsAttribute));

        // For non-flags enums, we want a zero value
        if (!isFlags)
        {
            var hasZeroValue = namedType.GetMembers()
                .OfType<IFieldSymbol>()
                .Any(f => f.HasConstantValue && (int)f.ConstantValue == 0);

            if (!hasZeroValue)
            {
                var location = namedType.Locations.FirstOrDefault();

                var syntaxReference = namedType.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxReference is not null)
                {
                    var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                    if (syntax is EnumDeclarationSyntax enumDeclaration)
                    {
                        location = enumDeclaration.Identifier.GetLocation();
                    }
                }

                var diag = Diagnostic.Create(
                    Rule,
                    location,
                    namedType.Name);
                context.ReportDiagnostic(diag);
            }
        }
    }
}