using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewUseOfInt64BitsToDoubleAnalyzer))]
public sealed class ReviewUseOfInt64BitsToDoubleAnalyzerTests
{
    [Fact]
    public async Task TestInt64BitsToDoubleUsage_WithIncorrectArgumentType_ShouldTriggerWarning()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        // This should trigger a warning as the argument is not Int64
        double result = BitConverter.Int64BitsToDouble(4);
    }
}";

        var context = new CSharpAnalyzerTest<ReviewUseOfInt64BitsToDoubleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewUseOfInt64BitsToDouble)
            .WithSpan(9, 25, 9, 58);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInt64BitsToDoubleUsage_WithCorrectArgumentType_ShouldNotTriggerWarning()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        long arg = 0L; // Correct argument type
        double result = BitConverter.Int64BitsToDouble(arg);
    }
}";

        var context = new CSharpAnalyzerTest<ReviewUseOfInt64BitsToDoubleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No expected diagnostics
    }
}