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

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(startContext =>
        {
            var serializableInterface = startContext.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.ISerializable");
            var serializationInfoType = startContext.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.SerializationInfo");
            var streamingContextType = startContext.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.StreamingContext");

            if (serializableInterface is null || serializationInfoType is null || streamingContextType is null)
            {
                return;
            }

            startContext.RegisterOperationBlockStartAction(blockStartContext =>
            {
                if (blockStartContext.OwningSymbol is not IMethodSymbol methodSymbol)
                {
                    return;
                }

                if (methodSymbol.ContainingType is not { } containingType)
                {
                    return;
                }

                if (!ImplementsSerializationPattern(containingType, serializableInterface))
                {
                    return;
                }

                if (IsSerializationConstructor(methodSymbol, serializationInfoType, streamingContextType))
                {
                    TrackSerializationConstructor(blockStartContext, methodSymbol, containingType, serializationInfoType, streamingContextType);
                    return;
                }

                if (IsGetObjectData(methodSymbol, serializationInfoType, streamingContextType))
                {
                    TrackGetObjectData(blockStartContext, methodSymbol, containingType, serializationInfoType, streamingContextType);
                }
            });
        });
    }

    private static void TrackSerializationConstructor(
        OperationBlockStartAnalysisContext blockStartContext,
        IMethodSymbol constructor,
        INamedTypeSymbol containingType,
        INamedTypeSymbol serializationInfoType,
        INamedTypeSymbol streamingContextType)
    {
        var callsBaseConstructor = false;

        blockStartContext.RegisterOperationAction(operationContext =>
        {
            var invocation = (IInvocationOperation)operationContext.Operation;

            if (invocation.TargetMethod is { MethodKind: MethodKind.Constructor } target &&
                SymbolEqualityComparer.Default.Equals(target.ContainingType, containingType.BaseType) &&
                IsSerializationConstructor(target, serializationInfoType, streamingContextType))
            {
                callsBaseConstructor = true;
            }
        }, OperationKind.Invocation);

        blockStartContext.RegisterOperationBlockEndAction(endContext =>
        {
            if (!callsBaseConstructor)
            {
                var diagnostic = Diagnostic.Create(Rule, constructor.Locations[0], containingType.Name, "constructor");
                endContext.ReportDiagnostic(diagnostic);
            }
        });
    }

    private static void TrackGetObjectData(
        OperationBlockStartAnalysisContext blockStartContext,
        IMethodSymbol method,
        INamedTypeSymbol containingType,
        INamedTypeSymbol serializationInfoType,
        INamedTypeSymbol streamingContextType)
    {
        var callsBaseMethod = false;

        blockStartContext.RegisterOperationAction(operationContext =>
        {
            var invocation = (IInvocationOperation)operationContext.Operation;

            if (invocation.TargetMethod is { Name: "GetObjectData" } target &&
                SymbolEqualityComparer.Default.Equals(target.ContainingType, containingType.BaseType) &&
                IsGetObjectData(target, serializationInfoType, streamingContextType))
            {
                callsBaseMethod = true;
            }
        }, OperationKind.Invocation);

        blockStartContext.RegisterOperationBlockEndAction(endContext =>
        {
            if (!callsBaseMethod)
            {
                var diagnostic = Diagnostic.Create(Rule, method.Locations[0], containingType.Name, "GetObjectData");
                endContext.ReportDiagnostic(diagnostic);
            }
        });
    }

    private static bool ImplementsSerializationPattern(INamedTypeSymbol type, INamedTypeSymbol serializableInterface)
    {
        return type.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, serializableInterface)) &&
               type.BaseType is { SpecialType: not SpecialType.System_Object };
    }

    private static bool IsSerializationConstructor(IMethodSymbol method, INamedTypeSymbol serializationInfoType, INamedTypeSymbol streamingContextType)
    {
        return method is { MethodKind: MethodKind.Constructor, Parameters.Length: 2 }
               && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, serializationInfoType)
               && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, streamingContextType);
    }

    private static bool IsGetObjectData(IMethodSymbol method, INamedTypeSymbol serializationInfoType, INamedTypeSymbol streamingContextType)
    {
        return method is { Name: "GetObjectData", MethodKind: MethodKind.Ordinary, Parameters.Length: 2 }
               && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, serializationInfoType)
               && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, streamingContextType);
    }
}
