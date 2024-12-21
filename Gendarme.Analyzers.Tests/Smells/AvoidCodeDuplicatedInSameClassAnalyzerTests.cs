using Gendarme.Analyzers.Smells;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidCodeDuplicatedInSameClassAnalyzer))]
public sealed class AvoidCodeDuplicatedInSameClassAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
