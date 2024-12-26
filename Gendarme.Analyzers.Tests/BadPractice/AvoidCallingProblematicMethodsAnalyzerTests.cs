using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(AvoidCallingProblematicMethodsAnalyzer))]
public sealed class AvoidCallingProblematicMethodsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
