using Gendarme.Analyzers.Extensions;

namespace Gendarme.Analyzers.Concurrency;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotLockOnWeakIdentityObjectsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotLockOnWeakIdentityObjectsAnalyzer_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotLockOnWeakIdentityObjects,
        Title,
        MessageFormat,
        Category.Concurrency,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    private static readonly ImmutableArray<string> ExceptionTypeMetadataNames =
    [
        "System.OutOfMemoryException",
        "System.ExecutionEngineException",
        "System.StackOverflowException"
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.LockStatement);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var lockStatement = (LockStatementSyntax)context.Node;
        var expression = lockStatement.Expression;

        var type = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken).Type;
        if (type is null)
        {
            return;
        }

        if (!IsWeakIdentityType(type, context.SemanticModel.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, expression.GetLocation()));
    }

    private static bool IsWeakIdentityType(ITypeSymbol type, Compilation compilation)
    {
        if (type is IErrorTypeSymbol)
        {
            return false;
        }

        if (type is ITypeParameterSymbol typeParameter)
        {
            foreach (var constraint in typeParameter.ConstraintTypes)
            {
                if (IsWeakIdentityType(constraint, compilation))
                {
                    return true;
                }
            }

            return false;
        }

        if (type.SpecialType == SpecialType.System_String)
        {
            return true;
        }

        if (IsOrInheritsFrom(type, compilation.GetTypeByMetadataName("System.Type")))
        {
            return true;
        }

        if (IsOrInheritsFrom(type, compilation.GetTypeByMetadataName("System.MarshalByRefObject")))
        {
            return true;
        }

        if (IsOrInheritsFrom(type, compilation.GetTypeByMetadataName("System.Threading.Thread")))
        {
            return true;
        }

        if (IsOrInheritsFrom(type, compilation.GetTypeByMetadataName("System.Reflection.MemberInfo")))
        {
            return true;
        }

        if (IsOrInheritsFrom(type, compilation.GetTypeByMetadataName("System.Reflection.ParameterInfo")))
        {
            return true;
        }

        foreach (var metadataName in ExceptionTypeMetadataNames)
        {
            if (IsOrInheritsFrom(type, compilation.GetTypeByMetadataName(metadataName)))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsOrInheritsFrom(ITypeSymbol type, INamedTypeSymbol? baseType)
    {
        if (baseType is null)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(type, baseType) || type.InheritsFrom(baseType);
    }
}
