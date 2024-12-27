using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(CheckParametersNullityInVisibleMethodsAnalyzer))]
public sealed class CheckParametersNullityInVisibleMethodsAnalyzerTests
{
    [Fact]
    public async Task TestPublicMethodWithNullParameter()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod(string param) { }
}";

        var context = new CSharpAnalyzerTest<CheckParametersNullityInVisibleMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CheckParametersNullityInVisibleMethods)
            .WithSpan(4, 26, 4, 38)
            .WithArguments("param");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicMethodWithNullCheck()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod(string param)
    {
        if (param == null) return;
    }
}";

        var context = new CSharpAnalyzerTest<CheckParametersNullityInVisibleMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestProtectedMethodWithNullParameter()
    {
        const string testCode = @"
public class MyClass
{
    protected void MyMethod(string param) { }
}";

        var context = new CSharpAnalyzerTest<CheckParametersNullityInVisibleMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CheckParametersNullityInVisibleMethods)
            .WithSpan(4, 29, 4, 41)
            .WithArguments("param");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPrivateMethodShouldNotReport()
    {
        const string testCode = @"
public class MyClass
{
    private void MyMethod(string param) { }
}";

        var context = new CSharpAnalyzerTest<CheckParametersNullityInVisibleMethodsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }
}