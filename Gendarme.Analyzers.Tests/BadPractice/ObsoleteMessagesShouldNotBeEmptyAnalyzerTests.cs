using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(ObsoleteMessagesShouldNotBeEmptyAnalyzer))]
public sealed class ObsoleteMessagesShouldNotBeEmptyAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
