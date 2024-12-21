using Gendarme.Analyzers.BadPractice;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(DisableDebuggingCodeAnalyzer))]
public sealed class DisableDebuggingCodeAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
