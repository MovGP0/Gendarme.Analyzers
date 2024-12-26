using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidComplexMethodsAnalyzer))]
public sealed class AvoidComplexMethodsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
