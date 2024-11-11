using Xunit.Abstractions;
using Xunit.Sdk;

namespace Gendarme.Analyzers.Tests;

public sealed class TestOfDiscoverer : ITraitDiscoverer
{
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        // Retrieve the "TypeName" property value from the attribute
        var typeName = traitAttribute.GetNamedArgument<string>("TypeName");
            
        // Define the trait key-value pair
        yield return new KeyValuePair<string, string>("Category", typeName);
    }
}