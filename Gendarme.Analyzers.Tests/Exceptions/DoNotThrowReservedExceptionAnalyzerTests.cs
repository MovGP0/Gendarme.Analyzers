using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(DoNotThrowReservedExceptionAnalyzer))]
public sealed class DoNotThrowReservedExceptionAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
