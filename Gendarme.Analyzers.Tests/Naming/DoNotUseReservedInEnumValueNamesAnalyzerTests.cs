using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(DoNotUseReservedInEnumValueNamesAnalyzer))]
public sealed class DoNotUseReservedInEnumValueNamesAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
