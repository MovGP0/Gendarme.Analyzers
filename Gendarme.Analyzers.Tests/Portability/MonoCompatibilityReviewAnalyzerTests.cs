using Gendarme.Analyzers.Portability;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(MonoCompatibilityReviewAnalyzer))]
public sealed class MonoCompatibilityReviewAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
