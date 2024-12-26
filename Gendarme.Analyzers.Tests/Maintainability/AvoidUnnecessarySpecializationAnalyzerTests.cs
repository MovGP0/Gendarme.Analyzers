using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(AvoidUnnecessarySpecializationAnalyzer))]
public sealed class AvoidUnnecessarySpecializationAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
