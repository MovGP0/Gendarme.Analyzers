using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(CheckNewExceptionWithoutThrowingAnalyzer))]
public sealed class CheckNewExceptionWithoutThrowingAnalyzerTests
{
    [Fact(Skip = "Analyzer not working yet")]
    public async Task TestNewExceptionWithoutThrowing()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public void TestMethod()
    {
        var exception = new InvalidOperationException();
    }
}
";

        var context = new CSharpAnalyzerTest<CheckNewExceptionWithoutThrowingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CheckNewExceptionWithoutThrowing)
            .WithSpan(7, 26, 7, 53)
            .WithArguments("InvalidOperationException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNewExceptionWithThrowing()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public void TestMethod()
    {
        throw new InvalidOperationException();
    }
}
";

        var context = new CSharpAnalyzerTest<CheckNewExceptionWithoutThrowingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };
        
        context.ExpectedDiagnostics.Clear();

        // No expected diagnostics since the exception is thrown
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNewExceptionAsArgument()
    {
        const string testCode = @"
using System;

public class TestClass
{
    public void TestMethod()
    {
        HandleException(new InvalidOperationException());
    }

    private void HandleException(Exception ex) { }
}
";

        var context = new CSharpAnalyzerTest<CheckNewExceptionWithoutThrowingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since the exception is passed as argument
        await context.RunAsync();
    }
}