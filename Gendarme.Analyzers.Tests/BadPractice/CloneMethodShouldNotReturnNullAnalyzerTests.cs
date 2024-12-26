using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(CloneMethodShouldNotReturnNullAnalyzer))]
public sealed class CloneMethodShouldNotReturnNullAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
