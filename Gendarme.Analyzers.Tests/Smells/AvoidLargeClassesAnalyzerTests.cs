using Gendarme.Analyzers.Smells;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidLargeClassesAnalyzer))]
public sealed class AvoidLargeClassesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
