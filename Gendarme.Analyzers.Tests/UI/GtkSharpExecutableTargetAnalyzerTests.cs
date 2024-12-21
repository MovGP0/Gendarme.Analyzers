using Gendarme.Analyzers.UI;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.UI;

[TestOf(typeof(GtkSharpExecutableTargetAnalyzer))]
public sealed class GtkSharpExecutableTargetAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
