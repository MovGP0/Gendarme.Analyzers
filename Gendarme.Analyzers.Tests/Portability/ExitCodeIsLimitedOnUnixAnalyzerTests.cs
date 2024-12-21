using Gendarme.Analyzers.Portability;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(ExitCodeIsLimitedOnUnixAnalyzer))]
public sealed class ExitCodeIsLimitedOnUnixAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
