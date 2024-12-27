using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewUselessControlFlowAnalyzer))]
public sealed class ReviewUselessControlFlowAnalyzerTests
{
    [Fact]
    public async Task TestEmptyBlock()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod()
            {
                { }
            }
        }";

        var context = new CSharpAnalyzerTest<ReviewUselessControlFlowAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewUselessControlFlow)
            .WithSpan(6, 17, 6, 20);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonEmptyBlock()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod()
            {
                var x = 1;
            }
        }";

        var context = new CSharpAnalyzerTest<ReviewUselessControlFlowAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // Expect no diagnostics for non-empty blocks
        context.ExpectedDiagnostics.Clear();
        
        await context.RunAsync();
    }
}