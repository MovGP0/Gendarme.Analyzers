using Gendarme.Analyzers.Portability;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(DoNotHardcodePathsAnalyzer))]
public sealed class DoNotHardcodePathsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
