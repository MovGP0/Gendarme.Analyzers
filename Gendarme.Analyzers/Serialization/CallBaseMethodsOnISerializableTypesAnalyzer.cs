using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Serialization;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CallBaseMethodsOnISerializableTypesAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.CallBaseMethodsOnISerializableTypesTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.CallBaseMethodsOnISerializableTypesMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.CallBaseMethodsOnISerializableTypesDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.CallBaseMethodsOnISerializableTypes,
        Title,
        MessageFormat,
        Category.Serialization,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        // Analyze method bodies
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
    }

    private void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        if (!namedType.AllInterfaces.Any(i => i.ToDisplayString() == "System.Runtime.Serialization.ISerializable"))
            return;

        if (namedType.BaseType == null || namedType.BaseType.SpecialType == SpecialType.System_Object)
            return;

        var serializationCtor = namedType.Constructors
            .FirstOrDefault(c => c.Parameters.Length == 2 &&
                                 c.Parameters[0].Type.ToDisplayString() == "System.Runtime.Serialization.SerializationInfo" &&
                                 c.Parameters[1].Type.ToDisplayString() == "System.Runtime.Serialization.StreamingContext");

        if (serializationCtor != null)
        {
            AnalyzeConstructor(context, serializationCtor, namedType);
        }

        var getObjectDataMethod = namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Name == "GetObjectData" &&
                                 m.Parameters.Length == 2 &&
                                 m.Parameters[0].Type.ToDisplayString() == "System.Runtime.Serialization.SerializationInfo" &&
                                 m.Parameters[1].Type.ToDisplayString() == "System.Runtime.Serialization.StreamingContext");

        if (getObjectDataMethod != null)
        {
            AnalyzeGetObjectDataMethod(context, getObjectDataMethod, namedType);
        }
    }

    private void AnalyzeConstructor(SymbolAnalysisContext context, IMethodSymbol constructor, INamedTypeSymbol namedType)
    {
        var callsBaseCtor = false;

        foreach (var syntaxRef in constructor.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax(context.CancellationToken);
            var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);

            var operation = semanticModel.GetOperation(syntax, context.CancellationToken) as IConstructorBodyOperation;

            if (operation != null && operation.Initializer is IExpressionStatementOperation expressionStatement &&
                expressionStatement.Operation is IInvocationOperation invocation &&
                invocation.TargetMethod.MethodKind == MethodKind.Constructor &&
                invocation.TargetMethod.ContainingType.Equals(namedType.BaseType))
            {
                callsBaseCtor = true;
                break;
            }
        }

        if (!callsBaseCtor)
        {
            var diagnostic = Diagnostic.Create(Rule, constructor.Locations[0], namedType.Name, "constructor");
            context.ReportDiagnostic(diagnostic);
        }
    }

    private void AnalyzeGetObjectDataMethod(SymbolAnalysisContext context, IMethodSymbol method, INamedTypeSymbol namedType)
    {
        var callsBaseMethod = false;

        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax(context.CancellationToken);
            var semanticModel = context.Compilation.GetSemanticModel(syntax.SyntaxTree);

            var operation = semanticModel.GetOperation(syntax, context.CancellationToken) as IMethodBodyOperation;

            if (operation != null)
            {
                var invocations = operation.Descendants().OfType<IInvocationOperation>();

                foreach (var invocation in invocations)
                {
                    if (invocation.TargetMethod.Name == "GetObjectData" &&
                        invocation.TargetMethod.ContainingType.Equals(namedType.BaseType))
                    {
                        callsBaseMethod = true;
                        break;
                    }
                }
            }

            if (callsBaseMethod)
                break;
        }

        if (!callsBaseMethod)
        {
            var diagnostic = Diagnostic.Create(Rule, method.Locations[0], namedType.Name, "GetObjectData");
            context.ReportDiagnostic(diagnostic);
        }
    }
}