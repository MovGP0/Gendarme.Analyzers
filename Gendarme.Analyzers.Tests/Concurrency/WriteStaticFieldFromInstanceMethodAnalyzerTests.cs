using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(WriteStaticFieldFromInstanceMethodAnalyzer))]
public sealed class WriteStaticFieldFromInstanceMethodAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
