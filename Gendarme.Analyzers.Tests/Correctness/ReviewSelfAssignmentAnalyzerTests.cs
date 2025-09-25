using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewSelfAssignmentAnalyzer))]
public sealed class ReviewSelfAssignmentAnalyzerTests
{
    [Fact]
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
            .WithSpan(7, 9, 7, 14)
            .WithArguments("x");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestParameterSelfAssignmentWarning()
    {
        const string testCode = @"
public class Bad
{
    private int value;

    public Bad(int value)
    {
        // argument is assigned to itself, this.value is unchanged
        value = value;
    }
}";

        var context = new CSharpAnalyzerTest<ReviewSelfAssignmentAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewSelfAssignment)
            .WithSpan(9, 9, 9, 22)
            .WithArguments("value");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }

    [Fact]
    public async Task TestFieldSelfAssignmentWarning()
    {
        const string testCode = @"
public class Bad
{
    private int value;

    public void Assign()
    {
        this.value = this.value;
    }
}";

        var context = new CSharpAnalyzerTest<ReviewSelfAssignmentAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewSelfAssignment)
            .WithSpan(8, 9, 8, 32)
            .WithArguments("this.value");

        context.ExpectedDiagnostics.Add(expected);
        await context.RunAsync();
    }

    [Fact]
    public async Task TestGoodAssignmentNoDiagnostic()
    {
        const string testCode = @"
public class Good
{
    private int value;

    public Good(int value)
    {
        this.value = value;
    }
}";

        var context = new CSharpAnalyzerTest<ReviewSelfAssignmentAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}