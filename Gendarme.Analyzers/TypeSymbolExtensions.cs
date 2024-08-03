namespace Gendarme.Analyzers;

public static class TypeSymbolExtensions
{
    public static bool IsIntegralType(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType switch
        {
            SpecialType.System_SByte or
            SpecialType.System_Byte or
            SpecialType.System_Int16 or
            SpecialType.System_UInt16 or
            SpecialType.System_Int32 or
            SpecialType.System_UInt32 or
            SpecialType.System_Int64 or
            SpecialType.System_UInt64 => true,
            _ => false,
        };
    }
}