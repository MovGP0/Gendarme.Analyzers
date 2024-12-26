using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(EqualsShouldHandleNullArgAnalyzer))]
public sealed class EqualsShouldHandleNullArgAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
