using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(GetEntryAssemblyMayReturnNullAnalyzer))]
public sealed class GetEntryAssemblyMayReturnNullAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
