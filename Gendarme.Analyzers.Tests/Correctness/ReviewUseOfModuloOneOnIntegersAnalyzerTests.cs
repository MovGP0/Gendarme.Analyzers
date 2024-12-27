using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(ReviewUseOfModuloOneOnIntegersAnalyzer))]
public sealed class ReviewUseOfModuloOneOnIntegersAnalyzerTests
{
    [Fact]
    public async Task TestModuloOneOnInteger()
    {
        const string testCode = @"
class TestClass
{
    void TestMethod()
    {
        int result = 10 % 1;
    }
}
";

        var context = new CSharpAnalyzerTest<ReviewUseOfModuloOneOnIntegersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewUseOfModuloOneOnIntegers)
            .WithSpan(6, 22, 6, 28);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestModuloOneOnNonIntegralType()
    {
        const string testCode = @"
class TestClass
{
    void TestMethod()
    {
        double result = 10.0 % 1;
    }
}
";

        var context = new CSharpAnalyzerTest<ReviewUseOfModuloOneOnIntegersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestModuloOtherValue()
    {
        const string testCode = @"
class TestClass
{
    void TestMethod()
    {
        int result = 10 % 2;
    }
}
";

        var context = new CSharpAnalyzerTest<ReviewUseOfModuloOneOnIntegersAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}