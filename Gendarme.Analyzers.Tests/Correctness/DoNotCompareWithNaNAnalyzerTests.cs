using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(DoNotCompareWithNaNAnalyzer))]
public sealed class DoNotCompareWithNaNAnalyzerTests
{
    [Fact]
    public async Task ReportsEqualityOperator()
    {
        const string source = @"
class C
{
    void M(double value)
    {
        if (value == double.NaN)
        {
        }
    }
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotCompareWithNaN)
            .WithSpan(6, 13, 6, 32);

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task ReportsInequalityOperator()
    {
        const string source = @"
class C
{
    void M(double value)
    {
        if (value != double.NaN)
        {
        }
    }
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotCompareWithNaN)
            .WithSpan(6, 13, 6, 32);

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task ReportsOrderingOperator()
    {
        const string source = @"
class C
{
    void M(float value)
    {
        if (float.NaN <= value)
        {
        }
    }
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotCompareWithNaN)
            .WithSpan(6, 13, 6, 31);

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task ReportsInstanceEquals()
    {
        const string source = @"
class C
{
    bool M(double value)
        => value.Equals(double.NaN);
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotCompareWithNaN)
            .WithSpan(5, 12, 5, 36);

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task ReportsStaticEquals()
    {
        const string source = @"
class C
{
    bool M(double value)
        => object.Equals(value, float.NaN);
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotCompareWithNaN)
            .WithSpan(5, 12, 5, 43);

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task SkipsIsNaNCall()
    {
        const string source = @"
class C
{
    bool M(double value)
        => double.IsNaN(value);
}
";

        await VerifyAsync(source);
    }

    [Fact]
    public async Task SkipsRegularComparison()
    {
        const string source = @"
class C
{
    bool M(double value)
        => value < 0;
}
";

        await VerifyAsync(source);
    }

    private static Task VerifyAsync(string source, params DiagnosticResult[] expectedDiagnostics)
    {
        var test = new CSharpAnalyzerTest<DoNotCompareWithNaNAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test.RunAsync();
    }
}


