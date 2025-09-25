using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(BadRecursiveInvocationAnalyzer))]
public sealed class BadRecursiveInvocationAnalyzerTests
{
    [Fact]
    public async Task ReportsRecursiveMethodCall()
    {
        const string source = @"
class C
{
    void M()
    {
        M();
    }
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.BadRecursiveInvocation)
            .WithSpan(6, 9, 6, 12)
            .WithArguments("M");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task ReportsExpressionBodiedMethod()
    {
        const string source = @"
class C
{
    void M() => M();
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.BadRecursiveInvocation)
            .WithSpan(4, 17, 4, 20)
            .WithArguments("M");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task ReportsRecursiveGetter()
    {
        const string source = @"
class C
{
    int P
    {
        get { return P; }
    }
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.BadRecursiveInvocation)
            .WithSpan(6, 22, 6, 23)
            .WithArguments("P");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task ReportsRecursiveSetter()
    {
        const string source = @"
class C
{
    private int _value;

    int P
    {
        get { return _value; }
        set { P = value; }
    }
}
";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.BadRecursiveInvocation)
            .WithSpan(9, 15, 9, 16)
            .WithArguments("P");

        await VerifyAsync(source, expected);
    }

    [Fact]
    public async Task SkipsNameofUsage()
    {
        const string source = @"
class C
{
    string M() => nameof(M);
}
";

        await VerifyAsync(source);
    }

    [Fact]
    public async Task SkipsPropertyUsingBackingField()
    {
        const string source = @"
class C
{
    private int _value;

    int P
    {
        get { return _value; }
        set { _value = value; }
    }
}
";

        await VerifyAsync(source);
    }

    private static Task VerifyAsync(string source, params DiagnosticResult[] expectedDiagnostics)
    {
        var test = new CSharpAnalyzerTest<BadRecursiveInvocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test.RunAsync();
    }
}
