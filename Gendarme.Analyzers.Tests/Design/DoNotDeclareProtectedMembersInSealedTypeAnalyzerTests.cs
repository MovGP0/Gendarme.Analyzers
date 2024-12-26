using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(DoNotDeclareProtectedMembersInSealedTypeAnalyzer))]
public sealed class DoNotDeclareProtectedMembersInSealedTypeAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
