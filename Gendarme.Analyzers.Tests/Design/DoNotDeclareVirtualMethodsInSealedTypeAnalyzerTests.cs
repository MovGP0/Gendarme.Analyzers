using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(DoNotDeclareVirtualMethodsInSealedTypeAnalyzer))]
public sealed class DoNotDeclareVirtualMethodsInSealedTypeAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
