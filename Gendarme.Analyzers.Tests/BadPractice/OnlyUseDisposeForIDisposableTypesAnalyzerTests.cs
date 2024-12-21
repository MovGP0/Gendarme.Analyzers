using Gendarme.Analyzers.BadPractice;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

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
