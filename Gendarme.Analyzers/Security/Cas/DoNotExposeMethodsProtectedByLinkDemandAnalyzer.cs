using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Security.Cas;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotExposeMethodsProtectedByLinkDemandAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.DoNotExposeMethodsProtectedByLinkDemandTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.DoNotExposeMethodsProtectedByLinkDemandMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.DoNotExposeMethodsProtectedByLinkDemandDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.DoNotExposeMethodsProtectedByLinkDemand,
        Title,
        MessageFormat,
        Category.Security,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method bodies
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;

        var targetMethod = invocation.TargetMethod;
        var callerMethod = invocation.SemanticModel.GetEnclosingSymbol(invocation.Syntax.SpanStart) as IMethodSymbol;

        if (targetMethod == null || callerMethod == null)
            return;

        // Check if the target method is protected by a LinkDemand
        var targetHasLinkDemand = HasLinkDemand(targetMethod.GetAttributes());

        if (!targetHasLinkDemand)
            return;

        // Check if the caller method has weaker security
        var callerSecurityLevel = GetSecurityLevel(callerMethod.GetAttributes());
        var targetSecurityLevel = GetSecurityLevel(targetMethod.GetAttributes());

        if (callerSecurityLevel < targetSecurityLevel)
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), callerMethod.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool HasLinkDemand(ImmutableArray<AttributeData> attributes)
    {
        return attributes.Any(attr => IsSecurityAttribute(attr) && GetSecurityAction(attr) == SecurityAction.LinkDemand);
    }

    private int GetSecurityLevel(ImmutableArray<AttributeData> attributes)
    {
        // Simplified security level determination
        if (attributes.Any(attr => IsSecurityAttribute(attr) && GetSecurityAction(attr) == SecurityAction.LinkDemand))
            return 2; // Higher security
        else
            return 1; // Lower security
    }

    private bool IsSecurityAttribute(AttributeData attribute)
    {
        var baseType = attribute.AttributeClass;
        while (baseType != null)
        {
            if (baseType.ToDisplayString() == "System.Security.Permissions.CodeAccessSecurityAttribute")
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private SecurityAction? GetSecurityAction(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length > 0)
        {
            var arg = attribute.ConstructorArguments[0];
            if (arg.Type.Name == "SecurityAction" && arg.Value != null)
            {
                return (SecurityAction)(int)arg.Value;
            }
        }
        return null;
    }
}