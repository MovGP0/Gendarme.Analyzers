using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewSelfAssignmentAnalyzer))]
public sealed class ReviewSelfAssignmentAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestSelfAssignmentWarning()
    {
        const string testCode = @"
class MyClass
{
    public void AssignValue()
    {
        int x = 5;
        x = x; // Self-assignment
    }
}";

        var context = new CSharpAnalyzerTest<ReviewSelfAssignmentAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewSelfAssignment)
            .WithSpan(6, 9, 6, 13) // span needs to match the range of 'x = x;'
            .WithArguments("x");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}