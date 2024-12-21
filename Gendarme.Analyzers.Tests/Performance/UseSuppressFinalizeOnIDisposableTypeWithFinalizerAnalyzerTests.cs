using Gendarme.Analyzers.Performance;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer))]
public sealed class UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
