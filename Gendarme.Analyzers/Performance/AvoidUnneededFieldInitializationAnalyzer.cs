namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule looks for constructors that assign fields to their default value
/// (e.g. 0 for an integer, null for an object or a string).
/// Since the CLR zero initializes all values there is no need, under most circumstances, to assign default values.
/// Doing so only adds size to source code and in IL.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public class Bad {
///     int i;
///     string s;
///  
///     public Bad ()
///     {
///         i = 0;
///         s = null;
///     }
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public class Good {
///     int i;
///     string s;
///  
///     public Good ()
///     {
///         // don't assign 'i' since it's already 0
///         // but we might prefer to assign a string to String.Empty
///         s = String.Empty;
///     }
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnneededFieldInitializationAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnneededFieldInitializationTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnneededFieldInitializationMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnneededFieldInitializationDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnneededFieldInitialization,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze field declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

        foreach (var variable in fieldDeclaration.Declaration.Variables)
        {
            if (variable.Initializer != null)
            {
                var semanticModel = context.SemanticModel;

                if (semanticModel.GetDeclaredSymbol(variable) is IFieldSymbol fieldSymbol && IsDefaultValue(variable.Initializer.Value, fieldSymbol.Type, context))
                {
                    var diagnostic = Diagnostic.Create(Rule, variable.Initializer.GetLocation(), fieldSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsDefaultValue(ExpressionSyntax expression, ITypeSymbol type, SyntaxNodeAnalysisContext context)
    {
        var operation = context.SemanticModel.GetOperation(expression, context.CancellationToken);
        var constantValue = operation?.ConstantValue;
        if (constantValue.HasValue)
        {
            var defaultValue = GetDefaultValue(type);
            return Equals(constantValue.Value, defaultValue);
        }
        return false;
    }

    private static object? GetDefaultValue(ITypeSymbol type)
    {
        return type.SpecialType switch
        {
            SpecialType.System_Boolean => false,
            SpecialType.System_Char => '\0',
            SpecialType.System_Byte
                or SpecialType.System_SByte
                or SpecialType.System_Int16
                or SpecialType.System_Int32
                or SpecialType.System_Int64
                or SpecialType.System_UInt16
                or SpecialType.System_UInt32
                or SpecialType.System_UInt64 => 0,
            SpecialType.System_Single
                or SpecialType.System_Double
                or SpecialType.System_Decimal => 0.0,
            _ => null
        };
    }
}