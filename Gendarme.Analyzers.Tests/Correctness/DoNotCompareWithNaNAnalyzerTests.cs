using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(DoNotCompareWithNaNAnalyzer))]
public sealed class DoNotCompareWithNaNAnalyzerTests
{
    [Fact]
    public async Task TestNaNComparison_Equality()
    {
        const string testCode = @"
class MyClass
{
    public void Check(double value)
    {
        if (value == double.NaN) // Warning should be triggered here
        {
        }
    }
}";

        var context = new CSharpAnalyzerTest<DoNotCompareWithNaNAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotCompareWithNaN)
            .WithSpan(6, 13, 6, 32);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNaNComparison_Inequality()
    {
        const string testCode = @"
class MyClass
{
    public void Check(double value)
    {
        if (value != double.NaN) // Warning should be triggered here
        {
        }
    }
}";

        var context = new CSharpAnalyzerTest<DoNotCompareWithNaNAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotCompareWithNaN)
            .WithSpan(5, 17, 5, 34);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoNaNComparison()
    {
        const string testCode = @"
class MyClass
{
    public void Check(double value)
    {
        if (value < 0) // No warning expected here
        {
        }
    }
}";

        var context = new CSharpAnalyzerTest<DoNotCompareWithNaNAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}