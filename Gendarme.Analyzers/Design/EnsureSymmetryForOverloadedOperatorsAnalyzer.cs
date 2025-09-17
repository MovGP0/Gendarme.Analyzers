using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnsureSymmetryForOverloadedOperatorsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.EnsureSymmetryForOverloadedOperatorsTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.EnsureSymmetryForOverloadedOperatorsMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.EnsureSymmetryForOverloadedOperatorsDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.EnsureSymmetryForOverloadedOperators,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly (string OperatorMetadata, string ComplementaryMetadata, string DisplaySymbol)[] OperatorPairs =
    [
        ("op_Addition", "op_Subtraction", "+"),
        ("op_Subtraction", "op_Addition", "-"),
        ("op_Equality", "op_Inequality", "=="),
        ("op_Inequality", "op_Equality", "!="),
        ("op_LessThan", "op_GreaterThan", "<"),
        ("op_GreaterThan", "op_LessThan", ">"),
        ("op_LessThanOrEqual", "op_GreaterThanOrEqual", "<="),
        ("op_GreaterThanOrEqual", "op_LessThanOrEqual", ">="),
    ];

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
        var operatorMethods = namedType.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.UserDefinedOperator)
            .GroupBy(m => m.MetadataName)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        foreach (var (operatorMetadata, complementaryMetadata, displaySymbol) in OperatorPairs)
        {
            if (!operatorMethods.TryGetValue(operatorMetadata, out var operatorSymbol))
            {
                continue;
            }

            if (operatorMethods.ContainsKey(complementaryMetadata))
            {
                continue;
            }

            var location = GetOperatorLocation(operatorSymbol, context.CancellationToken)
                ?? namedType.Locations.FirstOrDefault();

            if (location is null)
            {
                continue;
            }

            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                namedType.Name,
                displaySymbol);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static Location? GetOperatorLocation(IMethodSymbol operatorSymbol, CancellationToken cancellationToken)
    {
        var syntaxReference = operatorSymbol.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxReference is null)
        {
            return operatorSymbol.Locations.FirstOrDefault();
        }

        var syntax = syntaxReference.GetSyntax(cancellationToken);
        if (syntax is OperatorDeclarationSyntax operatorDeclaration)
        {
            return operatorDeclaration.OperatorToken.GetLocation();
        }

        return operatorSymbol.Locations.FirstOrDefault();
    }
}