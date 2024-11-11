using Gendarme.Analyzers.Extensions;

namespace Gendarme.Analyzers.Exceptions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MissingExceptionConstructorsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.MissingExceptionConstructorsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.MissingExceptionConstructorsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.MissingExceptionConstructorsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.MissingExceptionConstructors,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    private static readonly ImmutableArray<ConstructorSignature> RequiredConstructors =
    [
        new(Accessibility.Public, []),
        new(Accessibility.Public, [
            TypeSymbolCache.StringType
        ]),
        new(Accessibility.Public, [
            TypeSymbolCache.StringType,
            TypeSymbolCache.ExceptionType
        ]),
        new(Accessibility.Protected, [
            TypeSymbolCache.SerializationInfoType,
            TypeSymbolCache.StreamingContextType
        ])
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            // Cache commonly used type symbols
            var stringType = compilationContext.Compilation.GetSpecialType(SpecialType.System_String);
            var exceptionType = compilationContext.Compilation.GetTypeByMetadataName("System.Exception");
            var serializationInfoType = compilationContext.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.SerializationInfo");
            var streamingContextType = compilationContext.Compilation.GetTypeByMetadataName("System.Runtime.Serialization.StreamingContext");

            if (exceptionType == null || serializationInfoType == null || streamingContextType == null)
            {
                return;
            }

            // Initialize the type symbol cache
            TypeSymbolCache.Initialize(stringType, exceptionType, serializationInfoType, streamingContextType);

            compilationContext.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        });
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        if (!namedTypeSymbol.InheritsFrom("System.Exception"))
            return;

        var constructors = namedTypeSymbol.Constructors;
        var missingConstructors = RequiredConstructors.Where(rc => !ConstructorExists(constructors, rc)).ToList();

        if (missingConstructors.Any())
        {
            var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ConstructorExists(ImmutableArray<IMethodSymbol> constructors, ConstructorSignature signature)
    {
        return constructors.Any(c =>
            c.DeclaredAccessibility == signature.Accessibility &&
            ParametersMatch(c.Parameters, signature.ParameterTypes));
    }

    private static bool ParametersMatch(ImmutableArray<IParameterSymbol> parameters, ImmutableArray<ITypeSymbol> parameterTypes)
    {
        if (parameters.Length != parameterTypes.Length)
            return false;

        for (int i = 0; i < parameters.Length; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(parameters[i].Type, parameterTypes[i]))
                return false;
        }

        return true;
    }

    private record ConstructorSignature(
        Accessibility Accessibility,
        ImmutableArray<ITypeSymbol> ParameterTypes);

    private static class TypeSymbolCache
    {
        public static ITypeSymbol StringType { get; private set; } = null!;
        public static ITypeSymbol ExceptionType { get; private set; } = null!;
        public static ITypeSymbol SerializationInfoType { get; private set; } = null!;
        public static ITypeSymbol StreamingContextType { get; private set; } = null!;

        public static void Initialize(
            ITypeSymbol stringType,
            ITypeSymbol exceptionType,
            ITypeSymbol serializationInfoType,
            ITypeSymbol streamingContextType)
        {
            StringType = stringType;
            ExceptionType = exceptionType;
            SerializationInfoType = serializationInfoType;
            StreamingContextType = streamingContextType;
        }
    }
}