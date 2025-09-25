using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewCastOnIntegerMultiplicationAnalyzer))]
public sealed class ReviewCastOnIntegerMultiplicationAnalyzerTests
{
    [Fact]
    public async Task TestCastOnIntegerMultiplication()
    {
        const string testCode = @"
class TestClass
{
    void TestMethod()
    {
        int a = 10;
        int b = 20;
        long result = (long)(a * b);
    }
}";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerMultiplicationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewCastOnIntegerMultiplication)
            .WithMessage("Cast on integer multiplication may cause overflow")
            .WithSpan(8, 23, 8, 36);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoCastOnIntegerMultiplication()
    {
        const string testCode = @"
class TestClass
{
    void TestMethod()
    {
        int a = 10;
        int b = 20;
        var result = a * b;
    }
}";

        var context = new CSharpAnalyzerTest<ReviewCastOnIntegerMultiplicationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}