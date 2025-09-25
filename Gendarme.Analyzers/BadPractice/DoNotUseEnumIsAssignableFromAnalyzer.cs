namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotUseEnumIsAssignableFromAnalyzer : DiagnosticAnalyzer
{
    private const string EnumMetadataName = "System.Enum";
    private const string TypeMetadataName = "System.Type";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotUseEnumIsAssignableFrom_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotUseEnumIsAssignableFrom_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotUseEnumIsAssignableFrom_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotUseEnumIsAssignableFrom,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        if (!string.Equals(invocation.TargetMethod.Name, nameof(Type.IsAssignableFrom), StringComparison.Ordinal))
        {
            return;
        }

        var typeType = context.Compilation.GetTypeByMetadataName(TypeMetadataName);
        if (typeType is null || !SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.ContainingType, typeType))
        {
            return;
        }

        if (invocation.Instance is not ITypeOfOperation instanceTypeOf)
        {
            return;
        }

        var enumType = context.Compilation.GetTypeByMetadataName(EnumMetadataName);
        if (enumType is null || !SymbolEqualityComparer.Default.Equals(instanceTypeOf.TypeOperand, enumType))
        {
            return;
        }

        if (invocation.Arguments.Length != 1)
        {
            return;
        }

        var argument = invocation.Arguments[0];
        if (argument.Value is not ITypeOfOperation { TypeOperand: { TypeKind: TypeKind.Enum } })
        {
            return;
        }

        var location = invocation.Syntax.GetLocation();
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, invocation.Syntax.ToString()));
    }
}
