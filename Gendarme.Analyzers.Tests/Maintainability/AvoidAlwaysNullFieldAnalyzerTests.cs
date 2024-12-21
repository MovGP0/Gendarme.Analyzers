using Gendarme.Analyzers.Maintainability;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidAlwaysNullFieldAnalyzer))]
public sealed class AvoidAlwaysNullFieldAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
