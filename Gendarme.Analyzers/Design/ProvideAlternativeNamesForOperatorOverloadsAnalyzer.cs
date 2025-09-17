using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ProvideAlternativeNamesForOperatorOverloadsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.ProvideAlternativeNamesForOperatorOverloadsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.ProvideAlternativeNamesForOperatorOverloadsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.ProvideAlternativeNamesForOperatorOverloadsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.ProvideAlternativeNamesForOperatorOverloads,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    // Mapping from operator metadata name => suggested method
    private static readonly Dictionary<string, string> SOperatorToMethod = new()
    {
        { "op_UnaryPlus", "Plus" },
        { "op_UnaryNegation", "Negate" },
        { "op_LogicalNot", "LogicalNot" },
        { "op_OnesComplement", "OnesComplement" },
        { "op_Increment", "Increment" },
        { "op_Decrement", "Decrement" },
        { "op_True", "IsTrue" },
        { "op_False", "IsFalse" },
        { "op_Addition", "Add" },
        { "op_Subtraction", "Subtract" },
        { "op_Multiply", "Multiply" },
        { "op_Division", "Divide" },
        { "op_Modulus", "Modulus" },
        { "op_BitwiseAnd", "BitwiseAnd" },
        { "op_BitwiseOr", "BitwiseOr" },
        { "op_ExclusiveOr", "ExclusiveOr" },
        { "op_LeftShift", "LeftShift" },
        { "op_RightShift", "RightShift" },
        { "op_Equality", "Equals" },
        { "op_Inequality", "Equals" }, // Typically "not" equals, but we can use the same base name or "NotEquals"
        { "op_GreaterThan", "Compare" },
        { "op_LessThan", "Compare" },
        { "op_GreaterThanOrEqual", "Compare" },
        { "op_LessThanOrEqual", "Compare" },
    };

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
            .Where(m => m.MethodKind == MethodKind.UserDefinedOperator);

        foreach (var op in operatorMethods)
        {
            if (SOperatorToMethod.TryGetValue(op.Name, out var suggestedName))
            {
                // Check if there's a corresponding method with that name
                bool hasAlternative = namedType.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == suggestedName);

                if (!hasAlternative)
                {
                    var location = op.Locations.FirstOrDefault();

                    var syntaxReference = op.DeclaringSyntaxReferences.FirstOrDefault();
                    if (syntaxReference is not null)
                    {
                        var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                        if (syntax is OperatorDeclarationSyntax operatorDeclaration)
                        {
                            location = operatorDeclaration.OperatorKeyword.GetLocation();
                        }
                    }

                    var diag = Diagnostic.Create(
                        Rule,
                        location,
                        namedType.Name,
                        op.Name,
                        suggestedName);
                    context.ReportDiagnostic(diag);
                }
            }
        }
    }
}