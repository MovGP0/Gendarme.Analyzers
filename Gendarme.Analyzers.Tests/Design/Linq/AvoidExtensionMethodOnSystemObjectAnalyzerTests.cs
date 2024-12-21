using Gendarme.Analyzers.Design.Linq;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Design.Linq;

[TestOf(typeof(AvoidExtensionMethodOnSystemObjectAnalyzer))]
public sealed class AvoidExtensionMethodOnSystemObjectAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
