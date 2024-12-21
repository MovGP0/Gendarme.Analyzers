using Gendarme.Analyzers.Serialization;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(DeserializeOptionalFieldAnalyzer))]
public sealed class DeserializeOptionalFieldAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
