using Gendarme.Analyzers.Naming;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(UseCorrectSuffixAnalyzer))]
public sealed class UseCorrectSuffixAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
