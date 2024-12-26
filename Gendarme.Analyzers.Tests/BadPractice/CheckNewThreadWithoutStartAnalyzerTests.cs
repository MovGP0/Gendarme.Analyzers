namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(CheckNewThreadWithoutStartAnalyzer))]
public sealed class CheckNewThreadWithoutStartAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
