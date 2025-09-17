using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposableTypesShouldHaveFinalizerAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title =
        new LocalizableResourceString(nameof(Strings.DisposableTypesShouldHaveFinalizerTitle),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString MessageFormat =
        new LocalizableResourceString(nameof(Strings.DisposableTypesShouldHaveFinalizerMessage),
            Strings.ResourceManager, typeof(Strings));

    private static readonly LocalizableString Description =
        new LocalizableResourceString(nameof(Strings.DisposableTypesShouldHaveFinalizerDescription),
            Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DisposableTypesShouldHaveFinalizer,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        var implementsIDisposable = namedType.AllInterfaces.Any(i =>
            i.ToDisplayString() == "System.IDisposable");

        if (!implementsIDisposable)
        {
            return;
        }

        var hasFinalizer = namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(m => m.MethodKind == MethodKind.Destructor);

        var hasNativeField = namedType.GetMembers()
            .OfType<IFieldSymbol>()
            .Any(f => f is { IsStatic: false } && IsNativeFieldType(f.Type));

        if (hasNativeField && !hasFinalizer)
        {
            var location = namedType.Locations.FirstOrDefault();

            var syntaxReference = namedType.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference is not null)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is TypeDeclarationSyntax typeDeclaration)
                {
                    location = typeDeclaration.Identifier.GetLocation();
                }
            }

            var diagnostic = Diagnostic.Create(
                Rule,
                location,
                namedType.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsNativeFieldType(ITypeSymbol type)
    {
        if (type.SpecialType is SpecialType.System_IntPtr or SpecialType.System_UIntPtr)
        {
            return true;
        }

        return type is INamedTypeSymbol named &&
            named.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Runtime.InteropServices.HandleRef";
    }
}