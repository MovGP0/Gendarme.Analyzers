using Gendarme.Analyzers.Security;

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
