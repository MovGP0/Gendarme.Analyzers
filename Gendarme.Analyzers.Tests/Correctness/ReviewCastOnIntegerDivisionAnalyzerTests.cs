using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewCastOnIntegerDivisionAnalyzer))]
public sealed class ReviewCastOnIntegerDivisionAnalyzerTests
{
    [Fact]
    public async Task TestIntegerDivisionWithCast()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        double result = (double)5 / 2; // This should trigger a warning
    }
}
";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerDivisionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewCastOnIntegerDivision)
            .WithSpan(6, 29, 6, 34);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonIntegerDivisionWithoutCast()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        double result = 5.0 / 2; // This should not trigger a warning
    }
}
";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerDivisionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No warnings expected
    }

    [Fact]
    public async Task TestNoCastOnIntegerDivision()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        int result = 5 / 2; // This should not trigger a warning
    }
}
";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerDivisionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No warnings expected
    }
}