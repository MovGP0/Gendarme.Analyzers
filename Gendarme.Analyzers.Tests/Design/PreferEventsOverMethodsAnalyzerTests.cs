using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(PreferEventsOverMethodsAnalyzer))]
public sealed class PreferEventsOverMethodsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
