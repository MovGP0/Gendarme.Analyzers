using Xunit.Sdk;

namespace Gendarme.Analyzers.Tests;

[TraitDiscoverer("Gendarme.Analyzers.Tests.TestOfDiscoverer", "Gendarme.Analyzers.Tests")]
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class TestOfAttribute : Attribute, ITraitAttribute
{
    public string TypeName { get; }

    public TestOfAttribute(Type type)
    {
        TypeName = type.FullName ?? type.Name;
    }

    public TestOfAttribute(string typeName)
    {
        TypeName = typeName;
    }

    public TestOfAttribute(Type type, string objectName)
    {
        TypeName = (type.FullName ?? type.Name) + "." + objectName;
    }

    public TestOfAttribute(string typeName, string objectName)
    {
        TypeName = typeName + "." + objectName;
    }
}