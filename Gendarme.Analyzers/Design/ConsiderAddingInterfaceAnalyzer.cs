using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConsiderAddingInterfaceAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ConsiderAddingInterfaceTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ConsiderAddingInterfaceMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ConsiderAddingInterfaceDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ConsiderAddingInterface,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Info,  // Might be Info or Warning as you prefer
        isEnabledByDefault: true,
        description: Description
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes
        if (namedTypeSymbol.TypeKind != TypeKind.Class)
            return;

        // 1) Check if the type has a method "Do()"
        var doMethod = namedTypeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m is { Name: "Do", Parameters.Length: 0 });

        // 2) Check if the type implements IDoable
        var hasIDoable = namedTypeSymbol.AllInterfaces
            .Any(i => i.Name == "IDoable");

        if (doMethod is not null && !hasIDoable)
        {
            var location = doMethod.Locations.FirstOrDefault();

            var syntaxReference = doMethod.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference is not null)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is MethodDeclarationSyntax methodDeclaration)
                {
                    location = methodDeclaration.Identifier.GetLocation();
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