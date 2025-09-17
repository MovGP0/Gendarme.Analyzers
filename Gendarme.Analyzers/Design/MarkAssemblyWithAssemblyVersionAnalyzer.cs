namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarkAssemblyWithAssemblyVersionAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithAssemblyVersionTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithAssemblyVersionMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithAssemblyVersionDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MarkAssemblyWithAssemblyVersion,
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
        // We'll check assembly-level attributes in CompilationEnd
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var compilation = context.Compilation;

        // Does the assembly have an [AssemblyVersion] attribute?
        bool hasAssemblyVersion = compilation.Assembly.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == "System.Reflection.AssemblyVersionAttribute");

        if (!hasAssemblyVersion)
        {
            var diag = Diagnostic.Create(
                Rule,
                Location.None,
                compilation.AssemblyName ?? "<unnamed>");
            context.ReportDiagnostic(diag);
        }
    }
}