using Gendarme.Analyzers.Design;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(AvoidPropertiesWithoutGetAccessorAnalyzer))]
public sealed class AvoidPropertiesWithoutGetAccessorAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
