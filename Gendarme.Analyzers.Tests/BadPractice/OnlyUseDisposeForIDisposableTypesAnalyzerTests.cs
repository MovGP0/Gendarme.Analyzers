using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(OnlyUseDisposeForIDisposableTypesAnalyzer))]
public sealed class OnlyUseDisposeForIDisposableTypesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
