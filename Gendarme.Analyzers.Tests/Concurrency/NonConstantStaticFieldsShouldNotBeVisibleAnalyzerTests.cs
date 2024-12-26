using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(NonConstantStaticFieldsShouldNotBeVisibleAnalyzer))]
public sealed class NonConstantStaticFieldsShouldNotBeVisibleAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
