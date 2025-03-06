using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(AvoidNonAlphanumericIdentifierAnalyzer))]
public sealed class AvoidNonAlphanumericIdentifierAnalyzerTests
{
    [Fact]
    public async Task TestNonAlphanumericIdentifier()
    {
        const string testCode = """
class MyClass_ {}
""";

        var context = new CSharpAnalyzerTest<AvoidNonAlphanumericIdentifierAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidNonAlphanumericIdentifier)
            .WithSpan(1, 7, 1, 15)
            .WithArguments("MyClass_");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidIdentifier()
    {
        const string testCode = """
class MyClass {}
""";

        var context = new CSharpAnalyzerTest<AvoidNonAlphanumericIdentifierAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonAlphanumericNamespace()
    {
        const string testCode = """
namespace MyNamespace_ {}
""";

        var context = new CSharpAnalyzerTest<AvoidNonAlphanumericIdentifierAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidNonAlphanumericIdentifier)
            .WithSpan(1, 11, 1, 23)
            .WithArguments("MyNamespace_");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}