using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer))]
public sealed class ReviewLockUsedOnlyForOperationsOnVariablesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
