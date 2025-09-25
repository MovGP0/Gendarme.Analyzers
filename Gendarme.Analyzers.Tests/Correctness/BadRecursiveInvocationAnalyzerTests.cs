using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(BadRecursiveInvocationAnalyzer))]
public sealed class BadRecursiveInvocationAnalyzerTests
{
    [Fact]
    public async Task TestBadRecursiveMethodInvocation()
    {
        const string testCode = @"
public class MyClass
{
    public void MyMethod()
    {
        MyMethod(); // Recursive invocation
    }
}";

        var context = new CSharpAnalyzerTest<BadRecursiveInvocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.BadRecursiveInvocation)
            .WithSpan(6, 9, 6, 19)
            .WithArguments("MyMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestBadRecursivePropertyInvocation()
    {
        const string testCode = @"
public class MyClass
{
    public int MyProperty
    {
        get { return MyProperty; } // Recursive invocation
    }
}";

        var context = new CSharpAnalyzerTest<BadRecursiveInvocationAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.BadRecursiveInvocation)
            .WithSpan(5, 28, 5, 38)
            .WithArguments("MyProperty");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}