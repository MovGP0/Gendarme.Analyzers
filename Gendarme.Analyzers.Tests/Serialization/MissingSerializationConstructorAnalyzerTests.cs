using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(MissingSerializationConstructorAnalyzer))]
public sealed class MissingSerializationConstructorAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
