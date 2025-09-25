namespace Gendarme.Analyzers.Maintainability;

/// <summary>
/// This rule checks methods for over-specialized parameters.
/// E.g. parameter types that are unnecessarily specialized with respect to what the method needs to perform its job.
/// This often impairs the reusability of the method.
/// If a problem is found the rule will suggest the most general type, or interface, required for the method to work.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public class DefaultEqualityComparer : IEqualityComparer {
///     public int GetHashCode (object obj)
///     {
///         return o.GetHashCode ();
///     }
/// }
///  
/// public int Bad (DefaultEqualityComparer ec, object o)
/// {
///     return ec.GetHashCode (o);
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public class DefaultEqualityComparer : IEqualityComparer {
///     public int GetHashCode (object obj)
///     {
///         return o.GetHashCode ();
///     }
/// }
/// 
/// public int Good (IEqualityComparer ec, object o)
/// {
///     return ec.GetHashCode (o);
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnnecessarySpecializationAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnnecessarySpecializationTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnnecessarySpecializationMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnnecessarySpecializationDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnnecessarySpecialization,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Standard Roslyn analyzer initialization
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeParameterSyntax, SyntaxKind.Parameter);
    }

    private static void AnalyzeParameterSyntax(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ParameterSyntax { Type: {} parameterSyntaxType } parameterSyntax
            || context.SemanticModel.GetDeclaredSymbol(parameterSyntax) is not { ContainingSymbol: IMethodSymbol methodSymbol } parameterSymbol
            || parameterSymbol.Type.TypeKind == TypeKind.Interface
            || IsPrimitiveType(parameterSymbol.Type)
            || FindMinimalType(parameterSymbol, methodSymbol, context) is not {} minimalType
            || SymbolEqualityComparer.Default.Equals(minimalType, parameterSymbol.Type))
        {
            return;
        }

        var diagnostic = Diagnostic.Create(Rule, parameterSyntaxType.GetLocation(), parameterSymbol.Name, minimalType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsPrimitiveType(ITypeSymbol type)
    {
        return type.SpecialType != SpecialType.None && type.SpecialType <= SpecialType.System_UInt64;
    }

    private static ITypeSymbol? FindMinimalType(IParameterSymbol parameter, IMethodSymbol method, SyntaxNodeAnalysisContext context)
    {
        var usages = method.DeclaringSyntaxReferences.SelectMany(syntaxRef =>
        {
            var methodSyntax = syntaxRef.GetSyntax() as MethodDeclarationSyntax;
            return methodSyntax?.DescendantNodes().OfType<IdentifierNameSyntax>() ?? [];
        });

        var parameterUsages = usages.Where(u => SymbolEqualityComparer.Default.Equals(context.SemanticModel.GetSymbolInfo(u).Symbol, parameter));

        var accessedMembers = parameterUsages.SelectMany(u =>
        {
            if (u.Parent is not MemberAccessExpressionSyntax memberAccess)
                return [];

            var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
            return symbol != null ? [symbol] : Enumerable.Empty<ISymbol>();
        });

        var requiredMembers = accessedMembers.Select(m => m.Name).Distinct().ToArray();

        // If the parameter isn't used for any member access, do not attempt to generalize.
        if (requiredMembers.Length == 0)
            return null;

        // Find interfaces implemented by the parameter type that include all required members
        var interfaces = parameter.Type.AllInterfaces;

        foreach (var currentInterface in interfaces)
        {
            var currentInterfaceMembers = currentInterface.GetMembers().Select(m => m.Name);
            if (requiredMembers.All(rm => currentInterfaceMembers.Contains(rm)))
            {
                return currentInterface;
            }
        }

        return null;
    }
}
