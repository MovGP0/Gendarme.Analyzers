namespace Gendarme.Analyzers.UI;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseStaThreadAttributeOnSwfEntryPointsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseSTAThreadAttributeOnSWFEntryPointsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseSTAThreadAttributeOnSWFEntryPointsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseSTAThreadAttributeOnSWFEntryPointsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseStaThreadAttributeOnSwfEntryPoints,
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

        // Check if the assembly references System.Windows.Forms
        var referencesWinForms = compilation.ReferencedAssemblyNames
            .Any(assemblyName => assemblyName.Name.Equals("System.Windows.Forms", StringComparison.OrdinalIgnoreCase));

        if (!referencesWinForms)
        {
            return;
        }

        // Get the entry point method
        var entryPoint = compilation.GetEntryPoint(context.CancellationToken);

        if (entryPoint == null)
        {
            return;
        }

        var attributes = entryPoint.GetAttributes();

        var hasSTAThreadAttribute = attributes
            .Any(attr => attr.AttributeClass is { Name: "STAThreadAttribute" } &&
                         attr.AttributeClass.ContainingNamespace.ToDisplayString() == "System");

        var hasMTAThreadAttribute = attributes
            .Any(attr => attr.AttributeClass is { Name: "MTAThreadAttribute" } &&
                         attr.AttributeClass.ContainingNamespace.ToDisplayString() == "System");

        // If not decorated with [STAThread] or decorated with [MTAThread], report diagnostic
        if (!hasSTAThreadAttribute || hasMTAThreadAttribute)
        {
            var location = entryPoint.Locations.FirstOrDefault() ?? Location.None;
            var diagnostic = Diagnostic.Create(Rule, location);
            context.ReportDiagnostic(diagnostic);
        }
    }
}