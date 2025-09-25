namespace Gendarme.Analyzers.Security;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NativeFieldsShouldNotBeVisibleAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.NativeFieldsShouldNotBeVisibleTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.NativeFieldsShouldNotBeVisibleMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.NativeFieldsShouldNotBeVisibleDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.NativeFieldsShouldNotBeVisible,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
    }

    private void AnalyzeFieldSymbol(SymbolAnalysisContext context)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;

        // Only consider visible (public) fields
        if (fieldSymbol.DeclaredAccessibility != Accessibility.Public)
            return;

        var fieldType = fieldSymbol.Type;
        if (IsNativeType(fieldType))
        {
            var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name, fieldType.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNativeType(ITypeSymbol type)
    {
        // IntPtr / UIntPtr
        if (type.SpecialType == SpecialType.System_IntPtr || type.SpecialType == SpecialType.System_UIntPtr)
            return true;

        // System.Runtime.InteropServices.HandleRef
        if (IsType(type, "HandleRef", "System.Runtime.InteropServices"))
            return true;

        // System.Runtime.InteropServices.SafeHandle or any derived type
        if (IsOrDerivesFromSafeHandle(type))
            return true;

        return false;
    }

    private static bool IsOrDerivesFromSafeHandle(ITypeSymbol type)
    {
        for (var current = type; current is INamedTypeSymbol named; current = named.BaseType)
        {
            if (IsType(named, "SafeHandle", "System.Runtime.InteropServices"))
                return true;
        }

        return false;
    }

    private static bool IsType(ITypeSymbol symbol, string typeName, string containingNamespace)
    {
        return symbol.Name == typeName && symbol.ContainingNamespace?.ToDisplayString() == containingNamespace;
    }
}