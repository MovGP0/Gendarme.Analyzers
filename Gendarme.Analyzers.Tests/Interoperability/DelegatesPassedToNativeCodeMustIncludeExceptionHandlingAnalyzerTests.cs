using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzer))]
public sealed class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
