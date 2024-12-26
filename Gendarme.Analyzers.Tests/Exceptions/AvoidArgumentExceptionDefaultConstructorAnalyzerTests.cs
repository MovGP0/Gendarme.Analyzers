using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(AvoidArgumentExceptionDefaultConstructorAnalyzer))]
public sealed class AvoidArgumentExceptionDefaultConstructorAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
