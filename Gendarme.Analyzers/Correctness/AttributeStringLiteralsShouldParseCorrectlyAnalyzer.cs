using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Gendarme.Analyzers.Correctness;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AttributeStringLiteralsShouldParseCorrectlyAnalyzer : DiagnosticAnalyzer
{
    private const string StringSyntaxAttributeFullName = "System.Diagnostics.CodeAnalysis.StringSyntaxAttribute";

    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.AttributeStringLiteralsShouldParseCorrectly_Title), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.AttributeStringLiteralsShouldParseCorrectly_Message), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.AttributeStringLiteralsShouldParseCorrectly_Description), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.AttributeStringLiteralsShouldParseCorrectly,
        Title,
        MessageFormat,
        Category.Correctness,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attribute
            || context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol is not IMethodSymbol constructor
            || attribute.ArgumentList is not { } argumentList)
        {
            return;
        }

        for (var index = 0; index < argumentList.Arguments.Count; index++)
        {
            var argument = argumentList.Arguments[index];

            if (!TryGetConstantString(argument.Expression, context.SemanticModel, context.CancellationToken, out var literalValue))
            {
                continue;
            }

            var parseKind = GetParseKind(constructor, argument, index, context.SemanticModel, context.CancellationToken);
            if (parseKind is null)
            {
                continue;
            }

            if (IsValidLiteral(literalValue, parseKind.Value))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, argument.Expression.GetLocation(), literalValue));
        }
    }

    private static bool TryGetConstantString(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken, out string value)
    {
        var constantValue = semanticModel.GetConstantValue(expression, cancellationToken);
        if (constantValue.HasValue && constantValue.Value is string stringValue)
        {
            value = stringValue;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static ParseKind? GetParseKind(IMethodSymbol constructor, AttributeArgumentSyntax argument, int argumentIndex, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (argument.NameEquals is { } nameEquals)
        {
            var namedSymbol = semanticModel.GetSymbolInfo(nameEquals.Name, cancellationToken).Symbol;
            if (namedSymbol is IPropertySymbol property)
            {
                if (HasStringSyntaxAttribute(property))
                {
                    return null;
                }

                var fromName = ParseKindFromName(property.Name);
                if (fromName is not null)
                {
                    return fromName;
                }
            }

            if (namedSymbol is IFieldSymbol field)
            {
                if (HasStringSyntaxAttribute(field))
                {
                    return null;
                }

                var fromName = ParseKindFromName(field.Name);
                if (fromName is not null)
                {
                    return fromName;
                }
            }
        }

        var parameter = GetParameterSymbol(constructor, argument, argumentIndex);
        if (parameter is not null && parameter.Type.SpecialType == SpecialType.System_String)
        {
            if (HasStringSyntaxAttribute(parameter))
            {
                return null;
            }

            var fromName = ParseKindFromName(parameter.Name);
            if (fromName is not null)
            {
                return fromName;
            }
        }

        return ParseKindFromName(constructor.ContainingType.Name);
    }

    private static IParameterSymbol? GetParameterSymbol(IMethodSymbol constructor, AttributeArgumentSyntax argument, int argumentIndex)
    {
        if (argument.NameColon is { } nameColon)
        {
            var parameterName = nameColon.Name.Identifier.ValueText;
            return constructor.Parameters.FirstOrDefault(p => p.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase));
        }

        if (argument.NameEquals is null && argumentIndex < constructor.Parameters.Length)
        {
            return constructor.Parameters[argumentIndex];
        }

        return null;
    }

    private static ParseKind? ParseKindFromName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        if (name.IndexOf("version", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return ParseKind.Version;
        }

        if (name.IndexOf("guid", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return ParseKind.Guid;
        }

        if (name.IndexOf("uri", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("url", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return ParseKind.Uri;
        }

        return null;
    }

    private static bool HasStringSyntaxAttribute(ISymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == StringSyntaxAttributeFullName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsValidLiteral(string value, ParseKind kind)
    {
        return kind switch
        {
            ParseKind.Version => Version.TryParse(value, out _),
            ParseKind.Guid => Guid.TryParse(value, out _),
            ParseKind.Uri => Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute)
                && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out _),
            _ => true,
        };
    }

    private enum ParseKind
    {
        Version,
        Guid,
        Uri,
    }
}
