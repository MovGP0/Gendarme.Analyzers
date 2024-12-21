using Gendarme.Analyzers.Maintainability;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

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
