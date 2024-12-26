using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotUseLockedRegionOutsideMethodAnalyzer))]
public sealed class DoNotUseLockedRegionOutsideMethodAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
