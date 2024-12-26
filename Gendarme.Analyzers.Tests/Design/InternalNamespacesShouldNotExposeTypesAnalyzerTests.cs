using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(InternalNamespacesShouldNotExposeTypesAnalyzer))]
public sealed class InternalNamespacesShouldNotExposeTypesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
