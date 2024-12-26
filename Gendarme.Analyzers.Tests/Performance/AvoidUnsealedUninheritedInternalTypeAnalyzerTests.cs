using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUnsealedUninheritedInternalTypeAnalyzer))]
public sealed class AvoidUnsealedUninheritedInternalTypeAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
