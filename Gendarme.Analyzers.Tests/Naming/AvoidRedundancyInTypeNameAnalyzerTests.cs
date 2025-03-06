using Gendarme.Analyzers.Naming;
using Microsoft.CodeAnalysis;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(AvoidRedundancyInTypeNameAnalyzer))]
public sealed class AvoidRedundancyInTypeNameAnalyzerTests
{
    [Fact]
    public async Task TestRedundantTypeName()
    {
        const string testCode = @"
namespace MyNamespace
{
    public class MyNamespaceClass { }
}
";

        var context = new CSharpAnalyzerTest<AvoidRedundancyInTypeNameAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidRedundancyInTypeName, DiagnosticSeverity.Info)
            .WithSpan(4, 18, 4, 34)
            .WithArguments("MyNamespaceClass", "MyNamespace");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonRedundantTypeName()
    {
        const string testCode = @"
namespace MyNamespace
{
    public class AnotherClass { }
}
";

        var context = new CSharpAnalyzerTest<AvoidRedundancyInTypeNameAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for non-redundant type names
        await context.RunAsync();
    }
}