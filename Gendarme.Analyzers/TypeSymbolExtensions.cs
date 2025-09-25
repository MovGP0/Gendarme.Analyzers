namespace Gendarme.Analyzers;

public static class TypeSymbolExtensions
{
    /// <summary>
    /// Checks if a given type symbol represents an integral type.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the type symbol is any of the integral types
    /// (<c>sbyte</c>, <c>byte</c>, <c>short</c>, <c>ushort</c>, <c>int</c>, <c>uint</c>, <c>long</c>, or <c>ulong</c>).
    /// </returns>
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