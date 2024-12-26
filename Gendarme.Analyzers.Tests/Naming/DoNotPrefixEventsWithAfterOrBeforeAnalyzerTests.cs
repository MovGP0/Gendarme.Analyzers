using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(DoNotPrefixEventsWithAfterOrBeforeAnalyzer))]
public sealed class DoNotPrefixEventsWithAfterOrBeforeAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
