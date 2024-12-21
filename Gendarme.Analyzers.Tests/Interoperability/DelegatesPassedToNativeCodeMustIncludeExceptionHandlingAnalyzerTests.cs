using Gendarme.Analyzers.Interoperability;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzer))]
public sealed class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
