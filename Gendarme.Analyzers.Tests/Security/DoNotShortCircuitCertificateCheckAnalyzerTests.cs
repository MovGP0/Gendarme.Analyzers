using Gendarme.Analyzers.Security;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Security;

[TestOf(typeof(DoNotShortCircuitCertificateCheckAnalyzer))]
public sealed class DoNotShortCircuitCertificateCheckAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
