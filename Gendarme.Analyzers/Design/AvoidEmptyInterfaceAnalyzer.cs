using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidEmptyInterfaceAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidEmptyInterfaceTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidEmptyInterfaceMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidEmptyInterfaceDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidEmptyInterface,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeInterface, SymbolKind.NamedType);
    }

    private static void AnalyzeInterface(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.TypeKind == TypeKind.Interface)
        {
            // If the interface has no members, report
            if (namedTypeSymbol.GetMembers().Length == 0)
            {
                var location = namedTypeSymbol.Locations.FirstOrDefault();

                var syntaxReference = namedTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxReference is not null)
                {
                    var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                    if (syntax is InterfaceDeclarationSyntax interfaceDeclaration)
                    {
                        location = interfaceDeclaration.Identifier.GetLocation();
                    }
                }

                var diagnostic = Diagnostic.Create(
                    Rule,
                    location,
                    namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}