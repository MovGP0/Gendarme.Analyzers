namespace Gendarme.Analyzers.UI;

/// <summary>
/// An executable assembly, i.e. an <c>.exe</c>, refers to the gtk-sharp assembly but isn’t compiled using <c>-target:winexe</c>.
/// A console window will be created and shown under Windows (MS runtime) when the application is executed.
/// </summary>
/// <example>
/// Bad example:
/// <code>mcs gtk.cs -pkg:gtk-sharp</code>
/// Good example:
/// <code>mcs gtk.cs -pkg:gtk-sharp -target:winexe</code>
/// </example>
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

        // Check if the assembly references GtkSharp by name
        var referencesGtkSharp = compilation.ReferencedAssemblyNames
            .Any(assemblyName => assemblyName.Name.Equals("gtk-sharp", StringComparison.OrdinalIgnoreCase));

        // Or via a test-only assembly metadata flag
        if (!referencesGtkSharp)
        {
            var isGtkSharpApp = compilation.Assembly.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == typeof(System.Reflection.AssemblyMetadataAttribute).FullName
                          && a.ConstructorArguments.Length == 2
                          && a.ConstructorArguments[0].Value is string key && key == "IsGtkSharpApplication"
                          && a.ConstructorArguments[1].Value is string value && string.Equals(value, "True", StringComparison.OrdinalIgnoreCase));
            referencesGtkSharp = isGtkSharpApp;
        }

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