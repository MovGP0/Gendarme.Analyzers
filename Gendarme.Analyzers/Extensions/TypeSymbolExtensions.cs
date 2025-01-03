namespace Gendarme.Analyzers.Extensions;

internal static class TypeSymbolExtensions
{
    public static bool IsIntPtrType(this ITypeSymbol type) => type.ToString() == "System.IntPtr";
    public static bool IsUIntPtrType(this ITypeSymbol type) => type.ToString() == "System.UIntPtr";

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
}