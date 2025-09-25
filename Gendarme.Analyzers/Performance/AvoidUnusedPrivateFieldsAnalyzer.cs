namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule checks all private fields inside each type to see if some of them are not being used.
/// This could be a leftover from debugging or testing code or a more serious typo where a wrong field is being used.
/// In any case this makes the type bigger than it needs to be
/// which can affect performance when a large number of instances exist.
/// </summary>
/// <example>
/// Bad example:
/// <code language="C#">
/// public class Bad {
///     int level;
///     bool b;
///  
///     public void Indent ()
///     {
///         level++;
/// #if DEBUG
///         if (b) Console.WriteLine (level);
/// #endif
///     }
/// }
/// </code>
/// Good example:
/// <code language="C#">
/// public class Good {
///     int level;
/// #if DEBUG
///     bool b;
/// #endif
///  
///     public void Indent ()
///     {
///         level++;
/// #if DEBUG
///         if (b) Console.WriteLine (level);
/// #endif
///     }
/// }
/// </code>
/// </example>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidUnusedPrivateFieldsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AvoidUnusedPrivateFieldsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AvoidUnusedPrivateFieldsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AvoidUnusedPrivateFieldsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AvoidUnusedPrivateFields,
        Title,
        MessageFormat,
        Category.Maintainability,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            var referencedFields = new ConcurrentDictionary<IFieldSymbol, byte>(SymbolEqualityComparer.Default);

            startContext.RegisterOperationAction(operationContext =>
            {
                var fieldReference = (IFieldReferenceOperation)operationContext.Operation;
                referencedFields[fieldReference.Field] = 0;
            }, OperationKind.FieldReference);

            startContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;

                var privateFields = namedType.GetMembers().OfType<IFieldSymbol>()
                    .Where(f => f is { DeclaredAccessibility: Accessibility.Private, IsImplicitlyDeclared: false });

                foreach (var field in privateFields)
                {
                    if (referencedFields.ContainsKey(field))
                    {
                        continue;
                    }

                    var location = field.Locations.FirstOrDefault();
                    if (location is null)
                    {
                        continue;
                    }

                    var diagnostic = Diagnostic.Create(Rule, location, field.Name);
                    symbolContext.ReportDiagnostic(diagnostic);
                }
            }, SymbolKind.NamedType);
        });
    }
}
