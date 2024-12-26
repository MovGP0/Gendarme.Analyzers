using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotLockOnThisOrTypesAnalyzer))]
public sealed class DoNotLockOnThisOrTypesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
