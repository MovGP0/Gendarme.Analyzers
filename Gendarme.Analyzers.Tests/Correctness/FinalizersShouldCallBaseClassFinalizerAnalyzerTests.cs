using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(FinalizersShouldCallBaseClassFinalizerAnalyzer))]
public sealed class FinalizersShouldCallBaseClassFinalizerAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
