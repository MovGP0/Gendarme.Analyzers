using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(ImplementISerializableCorrectlyAnalyzer))]
public sealed class ImplementISerializableCorrectlyAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
