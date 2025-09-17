namespace Gendarme.Analyzers.Design;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseFlagsAttributeAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.UseFlagsAttributeTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.UseFlagsAttributeMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.UseFlagsAttributeDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.UseFlagsAttribute,
        Title,
        MessageFormat,
        Category.Design,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (namedType.TypeKind != TypeKind.Enum)
            return;

        // If the enum already has [Flags], skip
        bool hasFlags = namedType.GetAttributes()
            .Any(a => a.AttributeClass?.Name == nameof(FlagsAttribute));

        if (hasFlags)
            return;

        if (namedType.EnumUnderlyingType is not ITypeSymbol underlyingType)
            return;

        // Very naive check: if any fields are powers of two and more than one field => suggests bitmask usage
        var fields = namedType.GetMembers().OfType<IFieldSymbol>()
            .Where(static f => f.HasConstantValue)
            .ToList();

        int countBitfields = 0;
        foreach (var field in fields)
        {
            if (!TryGetPositiveFlagCandidate(field.ConstantValue, underlyingType, out var value))
                continue;

            if ((value & (value - 1)) == 0) // power-of-two check
            {
                countBitfields++;
                if (countBitfields > 1)
                    break;
            }
        }

        // If we found multiple power-of-two fields, it's likely a bitmask
        if (countBitfields > 1)
        {
            var location = namedType.Locations.FirstOrDefault();

            var syntaxReference = namedType.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxReference is not null)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is EnumDeclarationSyntax enumDeclaration)
                {
                    location = enumDeclaration.Identifier.GetLocation();
                }
            }

            var diag = Diagnostic.Create(
                Rule,
                location,
                namedType.Name);
            context.ReportDiagnostic(diag);
        }
    }

    private static bool TryGetPositiveFlagCandidate(object? constantValue, ITypeSymbol underlyingType, out ulong value)
    {
        value = 0;

        if (constantValue is null)
            return false;

        try
        {
            value = Convert.ToUInt64(constantValue);
        }
        catch (InvalidCastException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }

        if (value == 0)
            return false;

        if (IsSignedIntegralType(underlyingType))
        {
            long signedValue;

            try
            {
                signedValue = Convert.ToInt64(constantValue);
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
            catch (FormatException)
            {
                return false;
            }

            if (signedValue <= 0)
                return false;
        }

        return true;
    }

    private static bool IsSignedIntegralType(ITypeSymbol type)
        => type.SpecialType is SpecialType.System_SByte
        or SpecialType.System_Int16
        or SpecialType.System_Int32
        or SpecialType.System_Int64;
}