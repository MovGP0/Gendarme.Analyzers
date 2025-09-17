namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarkAssemblyWithComVisibleAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithComVisibleTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithComVisibleMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithComVisibleDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MarkAssemblyWithComVisible,
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
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var compilation = context.Compilation;

        bool hasComVisible = compilation.Assembly.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == "System.Runtime.InteropServices.ComVisibleAttribute");

        if (!hasComVisible)
        {
            // Try to grab the assembly title (if present) for the diagnostic argument
            // and report the diagnostic at a meaningful source location (prefer the title string literal).
            var titleAttr = compilation.Assembly.GetAttributes().FirstOrDefault(a =>
                a.AttributeClass?.ToDisplayString() == "System.Reflection.AssemblyTitleAttribute");

            string assemblyDisplayName = titleAttr is not null && titleAttr.ConstructorArguments.Length > 0
                ? titleAttr.ConstructorArguments[0].Value as string ?? (compilation.AssemblyName ?? "<unnamed>")
                : (compilation.AssemblyName ?? "<unnamed>");

            Location? location = null;
            var syntaxRef = titleAttr?.ApplicationSyntaxReference;
            if (syntaxRef is not null)
            {
                if (syntaxRef.GetSyntax(context.CancellationToken) is AttributeSyntax attributeSyntax)
                {
                    var firstArg = attributeSyntax.ArgumentList?.Arguments.FirstOrDefault();
                    var expr = firstArg?.Expression;
                    location = expr != null ? expr.GetLocation() : attributeSyntax.GetLocation();
                }
            }

            var diag = Diagnostic.Create(
                Rule,
                location ?? Location.None,
                assemblyDisplayName);
            context.ReportDiagnostic(diag);
        }
    }
}