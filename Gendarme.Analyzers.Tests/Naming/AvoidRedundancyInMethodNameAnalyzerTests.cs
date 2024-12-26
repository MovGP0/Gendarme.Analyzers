using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(AvoidRedundancyInMethodNameAnalyzer))]
public sealed class AvoidRedundancyInMethodNameAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
