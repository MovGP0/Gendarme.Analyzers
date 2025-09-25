namespace Gendarme.Analyzers.UI;

/// <summary>
/// An executable assembly, i.e. an <c>.exe</c>, refers to the <c>System.Windows.Forms</c> assembly but isn’t compiled using <c>-target:winexe</c>.
/// A console window will be created and shown under Windows (MS runtime) when the application is executed which is probably not desirable for a winforms application.
/// </summary>
/// <example>
/// Bad example:
/// <code>mcs swf.cs -pkg:dotnet</code>
/// Good example:
/// <code>mcs swf.cs -pkg:dotnet -target:winexe</code>
/// </example>
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
        description: Description,
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

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