namespace Gendarme.Analyzers.Extensions;

internal static class TypeSymbolExtensions
{
    public static bool IsIntPtrType(this ITypeSymbol type) => type?.SpecialType == SpecialType.System_IntPtr;
    public static bool IsUIntPtrType(this ITypeSymbol type) => type?.SpecialType == SpecialType.System_UIntPtr;

    public static bool Is32BitOrSmaller(this ITypeSymbol type)
    {
        return type.SpecialType
            is SpecialType.System_Int32
            or SpecialType.System_UInt32
            or SpecialType.System_Int16
            or SpecialType.System_UInt16
            or SpecialType.System_Byte
            or SpecialType.System_SByte;
    }

    public static bool InheritsFrom(this ITypeSymbol? symbol, ITypeSymbol baseType)
    {
        while (symbol != null)
        {
            if (SymbolEqualityComparer.Default.Equals(symbol.BaseType, baseType))
            {
                return true;
            }
            symbol = symbol.BaseType;
        }
        return false;
    }
}
