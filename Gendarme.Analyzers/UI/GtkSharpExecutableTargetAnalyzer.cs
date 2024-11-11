namespace Gendarme.Analyzers.UI;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GtkSharpExecutableTargetAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.GtkSharpExecutableTargetTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.GtkSharpExecutableTargetMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.GtkSharpExecutableTargetDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.GtkSharpExecutableTarget,
        Title,
        MessageFormat,
        Category.Ui,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        // Analyze the compilation as a whole
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
        var compilation = context.Compilation;

        // Check if the assembly is a console application
        if (compilation.Options.OutputKind != OutputKind.ConsoleApplication)
        {
            return;
        }

        // Check if the assembly references GtkSharp
        var referencesGtkSharp = compilation.ReferencedAssemblyNames
            .Any(assemblyName => assemblyName.Name.Equals("gtk-sharp", StringComparison.OrdinalIgnoreCase));

        if (!referencesGtkSharp)
        {
            return;
        }

        // Report diagnostic at the assembly symbol location
        var location = compilation.Assembly.Locations.FirstOrDefault() ?? Location.None;
        var diagnostic = Diagnostic.Create(Rule, location);
        context.ReportDiagnostic(diagnostic);
    }
}