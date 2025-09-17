using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ImplementICloneableCorrectlyAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.ImplementICloneableCorrectlyTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.ImplementICloneableCorrectlyMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.ImplementICloneableCorrectlyDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ImplementICloneableCorrectly,
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
        bool implementsICloneable = namedType.AllInterfaces.Any(i =>
            i.ToDisplayString() == "System.ICloneable");

        // Check for a method named 'Clone' returning 'object'
        var cloneMethod = namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m =>
                m is { Name: "Clone", IsStatic: false, Parameters.Length: 0 });

        if (cloneMethod is { ReturnType.SpecialType: SpecialType.System_Object })
        {
            // If it returns object but doesn't implement ICloneable -> diagnostic
            if (!implementsICloneable)
            {
                var location = cloneMethod.Locations.FirstOrDefault();

                var syntaxReference = cloneMethod.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxReference is not null)
                {
                    var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                    if (syntax is MethodDeclarationSyntax methodDeclaration)
                    {
                        location = methodDeclaration.Identifier.GetLocation();
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