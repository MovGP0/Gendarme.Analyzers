using Gendarme.Analyzers.Concurrency;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotUseThreadStaticWithInstanceFieldsAnalyzer))]
public sealed class DoNotUseThreadStaticWithInstanceFieldsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
