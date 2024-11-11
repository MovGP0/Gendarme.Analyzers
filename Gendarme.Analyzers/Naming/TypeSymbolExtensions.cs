namespace Gendarme.Analyzers.Naming;

internal static class TypeSymbolExtensions
{
    public static bool InheritsFromOrImplements(this ITypeSymbol typeSymbol, ITypeSymbol baseTypeOrInterface)
    {
        return typeSymbol.AllInterfaces.Contains(baseTypeOrInterface) || typeSymbol.InheritsFrom(baseTypeOrInterface);
    }

    public static bool InheritsFrom(this ITypeSymbol typeSymbol, ITypeSymbol baseType)
    {
        ITypeSymbol currentType = typeSymbol.BaseType;
        while (currentType != null)
        {
            if (SymbolEqualityComparer.Default.Equals(currentType, baseType))
                return true;
            currentType = currentType.BaseType;
        }
        return false;
    }
}