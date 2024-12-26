using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(PInvokeShouldNotBeVisibleAnalyzer))]
public sealed class PInvokeShouldNotBeVisibleAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
