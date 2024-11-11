namespace Gendarme.Analyzers.UI;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SystemWindowsFormsExecutableTargetAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.SystemWindowsFormsExecutableTargetTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.SystemWindowsFormsExecutableTargetMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.SystemWindowsFormsExecutableTargetDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.SystemWindowsFormsExecutableTarget,
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

        // Check if the assembly references System.Windows.Forms
        var referencesWinForms = compilation.ReferencedAssemblyNames
            .Any(assemblyName => assemblyName.Name.Equals("System.Windows.Forms", StringComparison.OrdinalIgnoreCase));

        if (!referencesWinForms)
        {
            return;
        }

        // Report diagnostic at the assembly symbol location
        var location = compilation.Assembly.Locations.FirstOrDefault() ?? Location.None;
        var diagnostic = Diagnostic.Create(Rule, location);
        context.ReportDiagnostic(diagnostic);
    }
}