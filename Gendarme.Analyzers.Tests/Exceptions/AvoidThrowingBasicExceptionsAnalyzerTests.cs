using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(AvoidThrowingBasicExceptionsAnalyzer))]
public sealed class AvoidThrowingBasicExceptionsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
