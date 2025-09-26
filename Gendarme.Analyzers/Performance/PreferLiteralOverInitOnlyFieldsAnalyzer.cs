namespace Gendarme.Analyzers.Performance;

/// <summary>
/// This rule looks for InitOnly fields (readonly in C#) that could be turned into Literal (const in C#)
/// because their value is known at compile time.
/// Literal fields don't need to be initialized (they don't force the compiler to add a static constructor),
/// resulting in less code and the value itself being used directly in IL.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferLiteralOverInitOnlyFieldsAnalyzer : DiagnosticAnalyzer
{
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsTitle), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsMessage), Strings.ResourceManager, typeof(Strings));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Strings.PreferLiteralOverInitOnlyFieldsDescription), Strings.ResourceManager, typeof(Strings));

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId.PreferLiteralOverInitOnlyFields,
        Title,
        MessageFormat,
        Category.Performance,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterOperationAction(static operationContext =>
        {
            var initializer = (IFieldInitializerOperation)operationContext.Operation;
            if (initializer.Value is not { ConstantValue.HasValue: true })
            {
                return;
            }

            foreach (var field in initializer.InitializedFields)
            {
                if (!ShouldAnalyze(field))
                {
                    continue;
                }

                if (!CanBeDeclaredConst(field.Type))
                {
                    continue;
                }

                var location = field.Locations.FirstOrDefault(static loc => loc.IsInSource);
                if (location is null)
                {
                    continue;
                }

                operationContext.ReportDiagnostic(Diagnostic.Create(Rule, location, field.Name));
            }
        }, OperationKind.FieldInitializer);
    }

    private static bool ShouldAnalyze(IFieldSymbol field) =>
        field is
        {
            IsStatic: true,
            IsReadOnly: true,
            IsConst: false,
            IsImplicitlyDeclared: false
        };

    private static bool CanBeDeclaredConst(ITypeSymbol type)
    {
        if (type.TypeKind is TypeKind.Enum)
        {
            return true;
        }

        return type.SpecialType is
            SpecialType.System_Boolean or
            SpecialType.System_Char or
            SpecialType.System_String or
            SpecialType.System_Byte or
            SpecialType.System_SByte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 or
            SpecialType.System_Single or
            SpecialType.System_Double or
            SpecialType.System_Decimal;
    }
}