namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseObjectDisposedExceptionAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseObjectDisposedExceptionTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseObjectDisposedExceptionMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseObjectDisposedExceptionDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseObjectDisposedException,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
        if (!typeSymbol.Interfaces.Any(i => i.ToString() == "System.IDisposable"))
            return;

        var disposeField = typeSymbol.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(f => f.Name == "disposed" && f.Type.SpecialType == SpecialType.System_Boolean);
        if (disposeField == null)
            return;

        foreach (var method in typeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (method.Name == "Dispose")
                continue;

            var methodSyntax = method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
            if (methodSyntax == null)
                continue;

            var body = methodSyntax.Body;
            if (body == null)
                continue;

            var statements = body.Statements;
            if (!statements.Any())
                continue;

            var containsDisposedCheck = statements.Any(s => s.ToString().Contains("disposed"));
            if (!containsDisposedCheck)
            {
                var diagnostic = Diagnostic.Create(Rule, methodSyntax.Identifier.GetLocation(), method.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}