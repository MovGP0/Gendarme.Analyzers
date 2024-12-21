using Gendarme.Analyzers.Security.Cas;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Security.Cas;

[TestOf(typeof(AddMissingTypeInheritanceDemandAnalyzer))]
public sealed class AddMissingTypeInheritanceDemandAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
