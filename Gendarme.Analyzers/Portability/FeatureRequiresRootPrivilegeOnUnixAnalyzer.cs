using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Portability;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FeatureRequiresRootPrivilegeOnUnixAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.FeatureRequiresRootPrivilegeOnUnixTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.FeatureRequiresRootPrivilegeOnUnixMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.FeatureRequiresRootPrivilegeOnUnixDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.FeatureRequiresRootPrivilegeOnUnix,
        Title,
        MessageFormat,
        Category.Portability,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze object creations and assignments
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(AnalyzeObjectCreation, OperationKind.ObjectCreation);
        context.RegisterOperationAction(AnalyzeAssignment, OperationKind.SimpleAssignment);
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context)
    {
        var objectCreation = (IObjectCreationOperation)context.Operation;

        if (objectCreation.Type.Name == "Ping" && objectCreation.Type.ContainingNamespace.ToDisplayString() == "System.Net.NetworkInformation")
        {
            var diagnostic = Diagnostic.Create(Rule, objectCreation.Syntax.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzeAssignment(OperationAnalysisContext context)
    {
        var assignment = (ISimpleAssignmentOperation)context.Operation;

        if (assignment.Target is IPropertyReferenceOperation propertyReference)
        {
            if (propertyReference.Property.Name == "PriorityClass" && propertyReference.Property.ContainingType.Name == "Process")
            {
                var assignedValue = assignment.Value;

                if (assignedValue != null)
                {
                    var constantValue = assignedValue.ConstantValue;
                    if (constantValue is { HasValue: true, Value: int intValue })
                    {
                        if (intValue != (int)System.Diagnostics.ProcessPriorityClass.Normal)
                        {
                            var diagnostic = Diagnostic.Create(Rule, assignment.Syntax.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                    else if (assignedValue is IFieldReferenceOperation fieldReference)
                    {
                        if (fieldReference.Field.Name != "Normal" && fieldReference.Field.ContainingType.Name == "ProcessPriorityClass")
                        {
                            var diagnostic = Diagnostic.Create(Rule, assignment.Syntax.GetLocation());
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}