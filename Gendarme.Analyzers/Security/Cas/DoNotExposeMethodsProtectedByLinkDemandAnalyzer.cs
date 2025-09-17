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

    private const string CodeAccessSecurityAttributeName = "System.Security.Permissions.CodeAccessSecurityAttribute";
    private const string SecurityActionTypeName = "System.Security.Permissions.SecurityAction";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation invocation)
        {
            return;
        }

        var targetMethod = invocation.TargetMethod;
        if (targetMethod is null)
        {
            return;
        }

        if (context.ContainingSymbol is not IMethodSymbol callerMethod)
        {
            return;
        }

        var codeAccessSecurityAttributeType = context.Compilation.GetTypeByMetadataName(CodeAccessSecurityAttributeName);
        var securityActionType = context.Compilation.GetTypeByMetadataName(SecurityActionTypeName);
        if (codeAccessSecurityAttributeType is null || securityActionType is null)
        {
            return;
        }

        if (!HasLinkDemand(targetMethod.GetAttributes(), codeAccessSecurityAttributeType, securityActionType))
        {
            return;
        }

        var callerSecurityLevel = GetSecurityLevel(callerMethod.GetAttributes(), codeAccessSecurityAttributeType, securityActionType);
        var targetSecurityLevel = GetSecurityLevel(targetMethod.GetAttributes(), codeAccessSecurityAttributeType, securityActionType);

        if (callerSecurityLevel < targetSecurityLevel)
        {
            var diagnostic = Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), callerMethod.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool HasLinkDemand(
        ImmutableArray<AttributeData> attributes,
        INamedTypeSymbol codeAccessSecurityAttributeType,
        INamedTypeSymbol securityActionType)
    {
        return attributes.Any(attribute =>
            IsSecurityAttribute(attribute, codeAccessSecurityAttributeType) &&
            GetSecurityAction(attribute, securityActionType) == SecurityAction.LinkDemand);
    }

    private static int GetSecurityLevel(
        ImmutableArray<AttributeData> attributes,
        INamedTypeSymbol codeAccessSecurityAttributeType,
        INamedTypeSymbol securityActionType)
    {
        return HasLinkDemand(attributes, codeAccessSecurityAttributeType, securityActionType) ? 2 : 1;
    }

    private static bool IsSecurityAttribute(AttributeData attribute, INamedTypeSymbol codeAccessSecurityAttributeType)
    {
        var current = attribute.AttributeClass;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, codeAccessSecurityAttributeType))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static SecurityAction? GetSecurityAction(AttributeData attribute, INamedTypeSymbol securityActionType)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        var argument = attribute.ConstructorArguments[0];
        if (argument.Type is not null &&
            SymbolEqualityComparer.Default.Equals(argument.Type, securityActionType) &&
            argument.Value is int value)
        {
            return (SecurityAction)value;
        }

        return null;
    }
}
