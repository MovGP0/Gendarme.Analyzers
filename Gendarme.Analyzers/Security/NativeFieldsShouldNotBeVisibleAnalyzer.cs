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

    private static readonly ImmutableHashSet<string> NativeTypes = ImmutableHashSet.Create(
        "System.IntPtr",
        "System.UIntPtr",
        "Microsoft.Win32.SafeHandles.SafeHandle",
        "System.Runtime.InteropServices.HandleRef"
    );

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze field declarations
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
    }

    private void AnalyzeFieldSymbol(SymbolAnalysisContext context)
    {
        var fieldSymbol = (IFieldSymbol)context.Symbol;

        // Check if the field is public
        if (fieldSymbol.DeclaredAccessibility != Accessibility.Public)
            return;

        // Check if the field type is a native type
        if (IsNativeType(fieldSymbol.Type))
        {
            var diagnostic = Diagnostic.Create(Rule, fieldSymbol.Locations[0], fieldSymbol.Name, fieldSymbol.Type.ToDisplayString());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsNativeType(ITypeSymbol type)
    {
        if (NativeTypes.Contains(type.ToDisplayString()))
            return true;

        // Check if the type inherits from SafeHandle
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.ToDisplayString() == "Microsoft.Win32.SafeHandles.SafeHandle")
                return true;

            baseType = baseType.BaseType;
        }

        return false;
    }
}