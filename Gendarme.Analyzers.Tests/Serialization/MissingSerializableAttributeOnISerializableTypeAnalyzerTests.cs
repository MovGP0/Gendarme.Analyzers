using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(MissingSerializableAttributeOnISerializableTypeAnalyzer))]
public sealed class MissingSerializableAttributeOnISerializableTypeAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
