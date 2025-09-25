using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewCastOnIntegerDivisionAnalyzer))]
public sealed class ReviewCastOnIntegerDivisionAnalyzerTests
{
    [Fact]
    public async Task Should_ReportWarning_When_ResultOfIntegerDivisionIsCastToDouble()
    {
        const string testCode = @"
using System;

public class Sample
{
    public double Compute(int numerator, int denominator)
    {
        return (double)(numerator / denominator);
    }
}";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerDivisionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewCastOnIntegerDivision)
            .WithSpan(8, 16, 8, 49);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task Should_ReportWarning_When_ResultOfIntegerDivisionIsCastToDecimal()
    {
        const string testCode = @"
using System;

public class Sample
{
    public decimal Compute(int numerator, int denominator)
    {
        return (decimal)(numerator / denominator);
    }
}";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerDivisionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewCastOnIntegerDivision)
            .WithSpan(8, 16, 8, 50);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task Should_NotReportWarning_When_OperandIsCastBeforeDivision()
    {
        const string testCode = @"
using System;

public class Sample
{
    public double Compute(int numerator, int denominator)
    {
        return (double)numerator / denominator;
    }
}";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerDivisionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task Should_NotReportWarning_When_DivisionIsAlreadyFloatingPoint()
    {
        const string testCode = @"
using System;

public class Sample
{
    public double Compute(int numerator, int denominator)
    {
        return (double)(numerator / (double)denominator);
    }
}";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerDivisionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}
