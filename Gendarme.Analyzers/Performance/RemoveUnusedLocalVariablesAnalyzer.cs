namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule flags locals that are declared but never used.
/// Unused locals increase IL size and can indicate logic errors.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemoveUnusedLocalVariablesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.RemoveUnusedLocalVariablesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.RemoveUnusedLocalVariables,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationBlockStartAction(operationBlockContext =>
        {
            if (operationBlockContext.OwningSymbol is not (IMethodSymbol or IPropertySymbol or IEventSymbol))
            {
                return;
            }

            var declaredLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
            var usedLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);

            operationBlockContext.RegisterOperationAction(operationContext =>
            {
                var declarator = (IVariableDeclaratorOperation)operationContext.Operation;
                if (declarator.Symbol is { IsConst: false, IsImplicitlyDeclared: false } local)
                {
                    declaredLocals.Add(local);
                }
            }, OperationKind.VariableDeclarator);

            operationBlockContext.RegisterOperationAction(operationContext =>
            {
                var localReference = (ILocalReferenceOperation)operationContext.Operation;
                if (localReference.IsDeclaration)
                {
                    return;
                }

                if (IsUsage(localReference))
                {
                    usedLocals.Add(localReference.Local);
                }
            }, OperationKind.LocalReference);

            operationBlockContext.RegisterOperationBlockEndAction(operationBlockEndContext =>
            {
                foreach (var local in declaredLocals)
                {
                    if (usedLocals.Contains(local))
                    {
                        continue;
                    }

                    var location = local.Locations.FirstOrDefault(static loc => loc.IsInSource);
                    if (location is null)
                    {
                        continue;
                    }

                    operationBlockEndContext.ReportDiagnostic(Diagnostic.Create(Rule, location, local.Name));
                }
            });
        });
    }

    private static bool IsUsage(ILocalReferenceOperation localReference)
    {
        var parent = localReference.Parent;
        if (parent is null)
        {
            return true;
        }

        if (parent is IAssignmentOperation assignment && ReferenceEquals(assignment.Target, localReference))
        {
            if (assignment is ISimpleAssignmentOperation { IsRef: false })
            {
                return false;
            }

            return true;
        }

        if (parent is IArgumentOperation argument)
        {
            return argument.Parameter?.RefKind switch
            {
                RefKind.Out => true,
                _ => true
            };
        }

        if (parent is IDiscardOperation)
        {
            return false;
        }

        return true;
    }
}