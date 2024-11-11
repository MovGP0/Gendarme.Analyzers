using Microsoft.CodeAnalysis.Operations;

namespace Gendarme.Analyzers.Extensions;

public static class ConversionOperationExtensions
{
    /// <summary>
    /// Determines if the conversion operation represents an unboxing conversion.
    /// </summary>
    /// <param name="conversion">The IConversionOperation to analyze.</param>
    /// <returns>True if the conversion is likely an unboxing operation; otherwise, false.</returns>
    public static bool IsUnboxingConversion(this IConversionOperation conversion)
    {
        if (conversion.Operand.Type is { } sourceType && conversion.Type is { } targetType)
        {
            // Unboxing occurs when converting from a reference type (e.g., object) to a value type
            return sourceType.IsReferenceType && targetType.IsValueType;
        }
        return false;
    }

    /// <summary>
    /// Determines if the conversion operation represents a try-cast pattern (e.g., "as" cast).
    /// </summary>
    /// <param name="conversion">The IConversionOperation to analyze.</param>
    /// <returns>True if the conversion is likely a try-cast; otherwise, false.</returns>
    public static bool IsTryCast(this IConversionOperation conversion)
    {
        if (conversion.Operand.Type is { } sourceType && conversion.Type is { } targetType)
        {
            // Try-cast occurs when casting explicitly from a reference type to another reference type
            // Ensure conversion is explicit
            return !conversion.IsImplicit && sourceType.IsReferenceType && targetType.IsReferenceType;
        }
        return false;
    }
}
