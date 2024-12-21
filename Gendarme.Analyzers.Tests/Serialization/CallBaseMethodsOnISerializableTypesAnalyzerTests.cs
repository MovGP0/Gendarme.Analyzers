using Gendarme.Analyzers.Serialization;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(CallBaseMethodsOnISerializableTypesAnalyzer))]
public sealed class CallBaseMethodsOnISerializableTypesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
