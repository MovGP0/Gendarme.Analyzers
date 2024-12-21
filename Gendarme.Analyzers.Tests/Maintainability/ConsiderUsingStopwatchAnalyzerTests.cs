using Gendarme.Analyzers.Maintainability;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(ConsiderUsingStopwatchAnalyzer))]
public sealed class ConsiderUsingStopwatchAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
