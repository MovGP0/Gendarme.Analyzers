using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(DoNotSwallowErrorsCatchingNonSpecificExceptionsAnalyzer))]
public sealed class DoNotSwallowErrorsCatchingNonSpecificExceptionsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
