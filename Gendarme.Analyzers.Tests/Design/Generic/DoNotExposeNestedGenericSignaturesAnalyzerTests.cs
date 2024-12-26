using Gendarme.Analyzers.Design.Generic;

namespace Gendarme.Analyzers.Tests.Design.Generic;

[TestOf(typeof(DoNotExposeNestedGenericSignaturesAnalyzer))]
public sealed class DoNotExposeNestedGenericSignaturesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
