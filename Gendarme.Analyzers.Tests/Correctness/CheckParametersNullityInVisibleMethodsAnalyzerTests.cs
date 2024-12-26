using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(CheckParametersNullityInVisibleMethodsAnalyzer))]
public sealed class CheckParametersNullityInVisibleMethodsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
