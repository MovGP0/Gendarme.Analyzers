using Gendarme.Analyzers.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

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

    private static ImmutableArray<ConstructorSignature> RequiredConstructors
    {
        get
        {
            // Only create the constructor signatures after TypeSymbolCache has been initialized
            return [
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
        }
    }

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
            // Get a valid location, defaulting to null if none available
            Location? location = null;
            if (namedTypeSymbol.Locations != null && namedTypeSymbol.Locations.Length > 0)
            {
                location = namedTypeSymbol.Locations[0];
            }

            var diagnostic = Diagnostic.Create(Rule, location, namedTypeSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ConstructorExists(
        ImmutableArray<IMethodSymbol> constructors, 
        ConstructorSignature signature)
    {
        return constructors.Any(c =>
        {
            bool accessibilityMatches = c.DeclaredAccessibility == signature.Accessibility;
            bool parametersMatch = ParametersMatch(c.Parameters, signature.ParameterTypes);

            return accessibilityMatches && parametersMatch;
        });
    }

    private static bool ParametersMatch(
        ImmutableArray<IParameterSymbol> parameters, 
        ImmutableArray<ITypeSymbol> parameterTypes)
    {
        if (parameters.Length != parameterTypes.Length)
            return false;

        for (int i = 0; i < parameters.Length; i++)
        {
            var actualType = parameters[i].Type;
            var expectedType = parameterTypes[i];

            // First try direct equality
            bool typesEqual = SymbolEqualityComparer.Default.Equals(actualType, expectedType);

            // If direct equality fails, try matching by name for serialization types
            if (!typesEqual)
            {
                // Special handling for SerializationInfo and StreamingContext 
                bool isSerializationInfoMatch = 
                    (actualType.MetadataName == "SerializationInfo" && expectedType.MetadataName == "SerializationInfo") ||
                    (actualType.ToDisplayString().EndsWith("SerializationInfo") && expectedType.ToDisplayString().EndsWith("SerializationInfo"));
                    
                bool isStreamingContextMatch = 
                    (actualType.MetadataName == "StreamingContext" && expectedType.MetadataName == "StreamingContext") ||
                    (actualType.ToDisplayString().EndsWith("StreamingContext") && expectedType.ToDisplayString().EndsWith("StreamingContext"));

                // Check if it's one of our special types and a match was found
                if ((TypeSymbolCache.SerializationInfoType != null && SymbolEqualityComparer.Default.Equals(expectedType, TypeSymbolCache.SerializationInfoType) && isSerializationInfoMatch) ||
                    (TypeSymbolCache.StreamingContextType != null && SymbolEqualityComparer.Default.Equals(expectedType, TypeSymbolCache.StreamingContextType) && isStreamingContextMatch))
                {
                    typesEqual = true;
                }
            }
            
            if (!typesEqual)
            {
                return false;
            }
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