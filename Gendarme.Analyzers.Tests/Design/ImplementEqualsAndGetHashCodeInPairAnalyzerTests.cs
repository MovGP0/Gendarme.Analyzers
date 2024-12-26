using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ImplementEqualsAndGetHashCodeInPairAnalyzer))]
public sealed class ImplementEqualsAndGetHashCodeInPairAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
