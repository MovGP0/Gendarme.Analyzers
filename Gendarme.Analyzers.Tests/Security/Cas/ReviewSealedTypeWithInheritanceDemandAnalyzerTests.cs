using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(ReviewSealedTypeWithInheritanceDemandAnalyzer))]
public sealed class ReviewSealedTypeWithInheritanceDemandAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
