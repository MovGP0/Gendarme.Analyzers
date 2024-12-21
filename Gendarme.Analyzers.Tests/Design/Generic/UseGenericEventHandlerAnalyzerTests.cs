using Gendarme.Analyzers.Design.Generic;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Design.Generic;

[TestOf(typeof(UseGenericEventHandlerAnalyzer))]
public sealed class UseGenericEventHandlerAnalyzerTests
{
    [Fact(Skip = "not implemented")]
    public async Task Foo()
    {
        throw new NotImplementedException();
    }
}
