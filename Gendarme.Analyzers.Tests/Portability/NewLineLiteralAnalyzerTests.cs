using Gendarme.Analyzers.Portability;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(NewLineLiteralAnalyzer))]
public sealed class NewLineLiteralAnalyzerTests
{
    [Fact]
    public async Task TestNewLineLiteral()
    {
        const string testCode = @"
    public class MyClass
    {
        public void MyMethod()
        {
            string message = ""Hello\nWorld"";
            string anotherMessage = ""Hello\rWorld"";
        }
    }";

        var context = new CSharpAnalyzerTest<NewLineLiteralAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.NewLineLiteral)
            .WithSpan(6, 30, 6, 44);

        var expected2 = DiagnosticResult
            .CompilerWarning(DiagnosticId.NewLineLiteral)
            .WithSpan(7, 37, 7, 51);

        context.ExpectedDiagnostics.Add(expected1);
        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoNewLineLiteral()
    {
        const string testCode = @"
    public class MyClass
    {
        public void MyMethod()
        {
            string message = ""Hello World"";
        }
    }";

        var context = new CSharpAnalyzerTest<NewLineLiteralAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}