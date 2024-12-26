using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(ImplementICloneableCorrectlyAnalyzer))]
public sealed class ImplementICloneableCorrectlyAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
