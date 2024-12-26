using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(MissingAttributeUsageOnCustomAttributeAnalyzer))]
public sealed class MissingAttributeUsageOnCustomAttributeAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
