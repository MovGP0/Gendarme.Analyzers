using Gendarme.Analyzers.Correctness;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(DoNotRecurseInEqualityAnalyzer))]
public sealed class DoNotRecurseInEqualityAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
