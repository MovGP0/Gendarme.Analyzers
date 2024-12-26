using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidSpeculativeGeneralityAnalyzer))]
public sealed class AvoidSpeculativeGeneralityAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
