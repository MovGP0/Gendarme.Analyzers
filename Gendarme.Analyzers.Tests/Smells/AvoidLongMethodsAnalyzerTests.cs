using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidLongMethodsAnalyzer))]
public sealed class AvoidLongMethodsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
