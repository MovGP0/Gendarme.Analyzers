using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(DoNotRoundIntegersAnalyzer))]
public sealed class DoNotRoundIntegersAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
