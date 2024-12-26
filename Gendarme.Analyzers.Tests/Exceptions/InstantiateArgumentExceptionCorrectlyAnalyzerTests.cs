using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(InstantiateArgumentExceptionCorrectlyAnalyzer))]
public sealed class InstantiateArgumentExceptionCorrectlyAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
