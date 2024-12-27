using Gendarme.Analyzers.Naming;

namespace Gendarme.Analyzers.Tests.Naming;

[TestOf(typeof(AvoidNonAlphanumericIdentifierAnalyzer))]
public sealed class AvoidNonAlphanumericIdentifierAnalyzerTests
{
    [Fact]
    public async Task TestNonAlphanumericIdentifier()
    {
        const string testCode = @"
class MyClass@ {}
";
        
        var context = new CSharpAnalyzerTest<AvoidNonAlphanumericIdentifierAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidNonAlphanumericIdentifier)
            .WithSpan(1, 6, 1, 12)
            .WithArguments("MyClass@");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidIdentifier()
    {
        const string testCode = @"
class MyClass {}
";

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
        const string testCode = @"
namespace MyNamespace$ {}
";

        var context = new CSharpAnalyzerTest<AvoidNonAlphanumericIdentifierAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AvoidNonAlphanumericIdentifier)
            .WithSpan(1, 11, 1, 20)
            .WithArguments("MyNamespace$");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}