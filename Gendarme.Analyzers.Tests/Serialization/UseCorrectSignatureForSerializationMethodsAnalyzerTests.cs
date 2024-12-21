using Gendarme.Analyzers.Serialization;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(UseCorrectSignatureForSerializationMethodsAnalyzer))]
public sealed class UseCorrectSignatureForSerializationMethodsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
