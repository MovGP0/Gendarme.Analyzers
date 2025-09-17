namespace Gendarme.Analyzers.BadPractice;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidVisibleConstantFieldAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidVisibleConstantField_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidVisibleConstantField_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidVisibleConstantField_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidVisibleConstantField,
        Title,
        MessageFormat,
        Category.BadPractice,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        var variableDeclaration = fieldDeclaration.Declaration;
        
        // Check if the field is a constant
        if (!fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        // Check if the field is visible outside the assembly
        if (!IsFieldVisibleOutsideAssembly(fieldDeclaration))
        {
            return;
        }

        foreach (var variable in variableDeclaration.Variables)
        {
            var diagnostic = Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variable.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool IsFieldVisibleOutsideAssembly(FieldDeclarationSyntax fieldDeclaration)
    {
        // Check if the field is public or protected
        return fieldDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword) ||
               fieldDeclaration.Modifiers.Any(SyntaxKind.ProtectedKeyword);
    }
}