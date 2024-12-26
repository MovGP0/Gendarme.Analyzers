using Gendarme.Analyzers.Serialization;

namespace Gendarme.Analyzers.Tests.Serialization;

[TestOf(typeof(MarkAllNonSerializableFieldsAnalyzer))]
public sealed class MarkAllNonSerializableFieldsAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
