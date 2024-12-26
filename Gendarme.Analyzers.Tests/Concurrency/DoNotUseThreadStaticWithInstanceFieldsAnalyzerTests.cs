using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotUseThreadStaticWithInstanceFieldsAnalyzer))]
public sealed class DoNotUseThreadStaticWithInstanceFieldsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
