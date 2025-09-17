using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Maintainability;

/// <summary>
/// This analyzer checks every type for lack of cohesion between the fields and the methods.
/// Low cohesion is often a sign that a type is doing too many, different and unrelated things.
/// The cohesion score is given for each defect (higher is better).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidLackOfCohesionOfMethodsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidLackOfCohesionOfMethodsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidLackOfCohesionOfMethodsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidLackOfCohesionOfMethodsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidLackOfCohesionOfMethods,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private const int MinimumFieldCount = 5;
    private const int MinimumMethodCount = 5;
    private const double SuccessLowerLimit = 0.5;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolStartAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedTypeSymbol(SymbolStartAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        if (typeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        var instanceFields = typeSymbol.GetMembers().OfType<IFieldSymbol>().Where(field => !field.IsStatic).ToList();
        if (instanceFields.Count < MinimumFieldCount)
        {
            return;
        }

        var candidateMethods = typeSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(IsCandidateMethod)
            .ToImmutableArray();

        if (candidateMethods.Length < MinimumMethodCount)
        {
            return;
        }

        var location = typeSymbol.Locations.FirstOrDefault();
        if (location is null)
        {
            return;
        }

        var candidateMethodSet = new HashSet<IMethodSymbol>(candidateMethods, SymbolEqualityComparer.Default);
        var methodFieldSets = new ConcurrentDictionary<IMethodSymbol, ImmutableHashSet<string>>(SymbolEqualityComparer.Default);

        context.RegisterOperationBlockStartAction(operationBlockStartContext =>
        {
            if (operationBlockStartContext.OwningSymbol is not IMethodSymbol method ||
                !candidateMethodSet.Contains(method))
            {
                return;
            }

            var builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

            operationBlockStartContext.RegisterOperationAction(operationContext =>
            {
                if (operationContext.Operation is not IFieldReferenceOperation fieldReference)
                {
                    return;
                }

                var field = fieldReference.Field;
                if (field is { IsStatic: false } &&
                    SymbolEqualityComparer.Default.Equals(field.ContainingType, typeSymbol))
                {
                    builder.Add(field.Name);
                }
            }, OperationKind.FieldReference);

            operationBlockStartContext.RegisterOperationBlockEndAction(_ =>
            {
                methodFieldSets[method] = builder.ToImmutable();
            });
        });

        context.RegisterSymbolEndAction(symbolEndContext =>
        {
            var methodFieldSetList = candidateMethods
                .Select(method => methodFieldSets.TryGetValue(method, out var fields) ? fields : null)
                .Where(fields => fields is not null)
                .Select(fields => fields!)
                .ToList();

            if (methodFieldSetList.Count < MinimumMethodCount)
            {
                return;
            }

            var methodPairs = 0;
            var disjointMethodPairs = 0;

            for (var i = 0; i < methodFieldSetList.Count; i++)
            {
                for (var j = i + 1; j < methodFieldSetList.Count; j++)
                {
                    symbolEndContext.CancellationToken.ThrowIfCancellationRequested();

                    methodPairs++;

                    if (!methodFieldSetList[i].Overlaps(methodFieldSetList[j]))
                    {
                        disjointMethodPairs++;
                    }
                }
            }

            if (methodPairs == 0)
            {
                return;
            }

            var levelOfComplexity = (double)disjointMethodPairs / methodPairs;

            if (levelOfComplexity > SuccessLowerLimit)
            {
                var diagnostic = Diagnostic.Create(Rule, location, typeSymbol.Name, levelOfComplexity.ToString("F2"));
                symbolEndContext.ReportDiagnostic(diagnostic);
            }
        });
    }

    private static bool IsCandidateMethod(IMethodSymbol methodSymbol) =>
        methodSymbol is { IsStatic: false, IsAbstract: false, IsExtern: false, MethodKind: MethodKind.Ordinary };
}

