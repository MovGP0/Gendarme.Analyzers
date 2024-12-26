using Gendarme.Analyzers.Smells;

namespace Gendarme.Analyzers.Tests.Smells;

[TestOf(typeof(AvoidCodeDuplicatedInSameClassAnalyzer))]
public sealed class AvoidCodeDuplicatedInSameClassAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
