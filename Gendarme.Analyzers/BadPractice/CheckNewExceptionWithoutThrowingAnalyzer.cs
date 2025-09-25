using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CheckNewExceptionWithoutThrowingAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.CheckNewExceptionWithoutThrowing_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.CheckNewExceptionWithoutThrowing_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.CheckNewExceptionWithoutThrowing_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.CheckNewExceptionWithoutThrowing,
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
        context.RegisterOperationAction(AnalyzeObjectCreation, OperationKind.ObjectCreation);
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context)
    {
        if (context.Operation is not IObjectCreationOperation objectCreation
            || objectCreation.Type is not INamedTypeSymbol typeSymbol
            || !IsExceptionType(typeSymbol, context.Compilation))
        {
            return;
        }

        var usage = SkipImplicitConversions(objectCreation.Parent);

        if (usage is null)
        {
            Report(context, objectCreation, typeSymbol);
            return;
        }

        if (usage is IThrowOperation or IReturnOperation)
        {
            return;
        }

        if (usage is IArgumentOperation argument)
        {
            var owner = SkipImplicitConversions(argument.Parent);
            if (owner is IInvocationOperation or IDynamicInvocationOperation or IObjectCreationOperation)
            {
                return;
            }
        }

        Report(context, objectCreation, typeSymbol);
    }

    private static void Report(OperationAnalysisContext context, IObjectCreationOperation objectCreation, INamedTypeSymbol typeSymbol)
    {
        var diagnostic = Diagnostic.Create(Rule, objectCreation.Syntax.GetLocation(), typeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsExceptionType(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var exceptionType = compilation.GetTypeByMetadataName("System.Exception");
        if (exceptionType is null)
        {
            return false;
        }

        for (var current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, exceptionType))
            {
                return true;
            }
        }

        return false;
    }

    private static IOperation? SkipImplicitConversions(IOperation? operation)
    {
        while (operation is IConversionOperation { IsImplicit: true } conversion)
        {
            operation = conversion.Parent;
        }

        return operation;
    }
}
