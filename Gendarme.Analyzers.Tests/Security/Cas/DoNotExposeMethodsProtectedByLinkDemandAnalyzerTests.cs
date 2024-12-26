using Gendarme.Analyzers.Security.Cas;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(DoNotExposeMethodsProtectedByLinkDemandAnalyzer))]
public sealed class DoNotExposeMethodsProtectedByLinkDemandAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
