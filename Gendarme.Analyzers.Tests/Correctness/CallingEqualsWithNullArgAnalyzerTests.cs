using Gendarme.Analyzers.Correctness;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(CallingEqualsWithNullArgAnalyzer))]
public sealed class CallingEqualsWithNullArgAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
