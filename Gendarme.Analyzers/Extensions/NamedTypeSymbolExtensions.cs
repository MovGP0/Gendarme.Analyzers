namespace Gendarme.Analyzers.Extensions;

public static class NamedTypeSymbolExtensions
{
    public static bool InheritsFrom(this INamedTypeSymbol symbol, string baseTypeName)
    {
        var baseType = symbol.BaseType;
        while (baseType != null)
        {
            if (baseType.ToString() == baseTypeName)
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }
}