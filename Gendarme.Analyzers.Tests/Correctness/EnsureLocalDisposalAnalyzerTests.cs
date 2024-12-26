using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(EnsureLocalDisposalAnalyzer))]
public sealed class EnsureLocalDisposalAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
