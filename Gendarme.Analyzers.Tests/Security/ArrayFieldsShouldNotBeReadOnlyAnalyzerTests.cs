using Gendarme.Analyzers.Security;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Security;

[TestOf(typeof(ArrayFieldsShouldNotBeReadOnlyAnalyzer))]
public sealed class ArrayFieldsShouldNotBeReadOnlyAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
