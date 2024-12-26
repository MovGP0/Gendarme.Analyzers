using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(DoNotAssumeIntPtrSizeAnalyzer))]
public sealed class DoNotAssumeIntPtrSizeAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
