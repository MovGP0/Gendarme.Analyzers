using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Performance;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseTypeEmptyTypesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseTypeEmptyTypesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseTypeEmptyTypesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseTypeEmptyTypesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseTypeEmptyTypes,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze object creation expressions
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeArrayCreation, OperationKind.ArrayCreation);
    }

    private static void AnalyzeArrayCreation(OperationAnalysisContext context)
    {
        if (context.Operation is not IArrayCreationOperation { Type: IArrayTypeSymbol arrayType } arrayCreation
            || arrayType.ElementType.ToDisplayString() != "System.Type"
            || arrayCreation.DimensionSizes is not [{ ConstantValue: { HasValue: true, Value: 0 } }])
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, arrayCreation.Syntax.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}