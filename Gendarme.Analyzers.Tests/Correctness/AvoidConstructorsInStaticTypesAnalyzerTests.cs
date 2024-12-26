using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(AvoidConstructorsInStaticTypesAnalyzer))]
public sealed class AvoidConstructorsInStaticTypesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
