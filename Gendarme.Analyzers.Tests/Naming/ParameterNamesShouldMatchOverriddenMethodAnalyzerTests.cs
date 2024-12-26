using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(ParameterNamesShouldMatchOverriddenMethodAnalyzer))]
public sealed class ParameterNamesShouldMatchOverriddenMethodAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
