namespace Gendarme.Analyzers.Smells;

/// <summary>
/// This rule allows developers to avoid the Speculative Generality smell.
/// Be careful if you are developing a new framework or a new library, because this rule only inspects the assembly,
/// then if you provide an abstract base class for extend by third party people,
/// then the rule can warn you.
/// You can ignore the message in this special case. We detect this smell by looking for:
/// <ul>
/// <li>Abstract classes without responsibility</li>
/// <li>Unnecessary delegation.</li>
/// <li>Unused parameters.</li>
/// </ul>
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// // An abstract class with only one subclass.
/// public abstract class AbstractClass {
///     public abstract void MakeStuff ();
/// }
///  
/// public class OverriderClass : AbstractClass {
///     public override void MakeStuff ()
///     {
///     }
/// }
/// </code>
/// If you use Telephone class only in one client, perhaps you don’t need this kind of delegation.
/// <code language="C#">
/// public class Person {
///     int age;
///     string name;
///     Telephone phone;
/// }
///  
/// public class Telephone {
///     int areaCode;
///     int phone;
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public abstract class OtherAbstractClass{
///     public abstract void MakeStuff ();
/// }
///  
/// public class OtherOverriderClass : OtherAbstractClass {
///     public override void MakeStuff ()
///     {
///     }
/// }
///  
/// public class YetAnotherOverriderClass : OtherAbstractClass {
///     public override void MakeStuff ()
///     {
///     }
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidSpeculativeGeneralityAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidSpeculativeGeneralityTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidSpeculativeGeneralityMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidSpeculativeGeneralityDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidSpeculativeGenerality,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (namedTypeSymbol.TypeKind != TypeKind.Class || !namedTypeSymbol.IsAbstract)
        {
            return;
        }

        // Count direct subclasses of this abstract class within the current compilation
        var implementationsCount = GetAllNamedTypes(context.Compilation.GlobalNamespace)
            .Count(t => SymbolEqualityComparer.Default.Equals(t.BaseType, namedTypeSymbol));

        if (implementationsCount == 1 && namedTypeSymbol.Locations.Length > 0)
        {
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(INamespaceSymbol namespaceSymbol)
    {
        // Direct types in this namespace
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            foreach (var t in GetSelfAndNestedTypes(type))
                yield return t;
        }

        // Recurse into nested namespaces
        foreach (var ns in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var t in GetAllNamedTypes(ns))
                yield return t;
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetSelfAndNestedTypes(INamedTypeSymbol type)
    {
        yield return type;
        foreach (var nested in type.GetTypeMembers())
        {
            foreach (var t in GetSelfAndNestedTypes(nested))
                yield return t;
        }
    }
}
