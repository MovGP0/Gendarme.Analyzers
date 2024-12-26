using Gendarme.Analyzers.Portability;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(DoNotHardcodePathsAnalyzer))]
public sealed class DoNotHardcodePathsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
