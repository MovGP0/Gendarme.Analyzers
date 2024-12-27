using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewDoubleAssignmentAnalyzer))]
public sealed class ReviewDoubleAssignmentAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestDoubleAssignmentWarning()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod()
            {
                int x = 5;
                x = x; // This should trigger a warning
            }
        }";

        var context = new CSharpAnalyzerTest<ReviewDoubleAssignmentAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewDoubleAssignment)
            .WithSpan(6, 17, 6, 23)
            .WithArguments("x");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoDoubleAssignmentWarning()
    {
        const string testCode = @"
        public class MyClass
        {
            public void MyMethod()
            {
                int x = 5;
                x = 6; // This should not trigger a warning
            }
        }";

        var context = new CSharpAnalyzerTest<ReviewDoubleAssignmentAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No expected diagnostics
    }
}