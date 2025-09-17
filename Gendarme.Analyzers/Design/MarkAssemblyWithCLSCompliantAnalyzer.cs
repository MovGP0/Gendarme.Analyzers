namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MarkAssemblyWithClsCompliantAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithClsCompliantTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithClsCompliantMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MarkAssemblyWithClsCompliantDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MarkAssemblyWithClsCompliant,
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

        bool hasClsCompliant = compilation.Assembly.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == "System.CLSCompliantAttribute");

        if (!hasClsCompliant)
        {
            var diag = Diagnostic.Create(
                Rule,
                Location.None,
                compilation.AssemblyName ?? "<unnamed>");
            context.ReportDiagnostic(diag);
        }
    }
}