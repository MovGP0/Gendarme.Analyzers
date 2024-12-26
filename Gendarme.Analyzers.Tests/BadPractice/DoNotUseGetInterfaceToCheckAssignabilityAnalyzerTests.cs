using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(DoNotUseGetInterfaceToCheckAssignabilityAnalyzer))]
public sealed class DoNotUseGetInterfaceToCheckAssignabilityAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
