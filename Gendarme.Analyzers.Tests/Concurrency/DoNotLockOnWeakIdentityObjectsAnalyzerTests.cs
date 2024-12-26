using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotLockOnWeakIdentityObjectsAnalyzer))]
public sealed class DoNotLockOnWeakIdentityObjectsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
