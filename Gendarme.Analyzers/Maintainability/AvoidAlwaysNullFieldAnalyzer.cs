using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Maintainability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidAlwaysNullFieldAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidAlwaysNullFieldTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidAlwaysNullFieldMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidAlwaysNullFieldDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidAlwaysNullField,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(startContext =>
        {
            var assignedFields = new ConcurrentDictionary<IFieldSymbol, byte>(SymbolEqualityComparer.Default);

            startContext.RegisterOperationAction(operationContext =>
            {
                switch (operationContext.Operation)
                {
                    case ISimpleAssignmentOperation { Target: IFieldReferenceOperation fieldReference }:
                        assignedFields[fieldReference.Field] = 0;
                        break;
                    case ICompoundAssignmentOperation { Target: IFieldReferenceOperation fieldReference }:
                        assignedFields[fieldReference.Field] = 0;
                        break;
                    case ICoalesceAssignmentOperation { Target: IFieldReferenceOperation fieldReference }:
                        assignedFields[fieldReference.Field] = 0;
                        break;
                    case IIncrementOrDecrementOperation { Target: IFieldReferenceOperation fieldReference }:
                        assignedFields[fieldReference.Field] = 0;
                        break;
                }
            }, OperationKind.SimpleAssignment, OperationKind.CompoundAssignment, OperationKind.CoalesceAssignment, OperationKind.Increment, OperationKind.Decrement);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                var fieldSymbol = (IFieldSymbol)symbolContext.Symbol;

                if (fieldSymbol.DeclaredAccessibility != Accessibility.Private)
                {
                    return;
                }

                if (fieldSymbol.Type.IsValueType)
                {
                    return;
                }

                if (HasInitializer(fieldSymbol, symbolContext.CancellationToken))
                {
                    return;
                }

                if (assignedFields.ContainsKey(fieldSymbol))
                {
                    return;
                }

                var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name);
                symbolContext.ReportDiagnostic(diagnostic);
            }, SymbolKind.Field);
        });
    }

    private static bool HasInitializer(IFieldSymbol fieldSymbol, CancellationToken cancellationToken)
    {
        if (fieldSymbol.HasConstantValue)
        {
            return true;
        }

        foreach (var reference in fieldSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax(cancellationToken) is VariableDeclaratorSyntax { Initializer: not null })
            {
                return true;
            }
        }

        return false;
    }
}
