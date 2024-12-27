using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer))]
public sealed class ReviewLockUsedOnlyForOperationsOnVariablesAnalyzerTests
{
    [Fact]
    public async Task TestLockOnStaticFieldAssignment()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private static int _counter;

    public void MyMethod()
    {
        lock (this)
        {
            _counter = 0; // This should trigger a diagnostic
        }
    }
}
        ";

        var context = new CSharpAnalyzerTest<ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewLockUsedOnlyForOperationsOnVariables)
            .WithSpan(10, 15, 10, 19)
            .WithArguments("this");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestLockWithoutStaticFieldAssignment()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private int _counter;

    public void MyMethod()
    {
        lock (this)
        {
            _counter = 0; // This should not trigger a diagnostic
        }
    }
}
        ";

        var context = new CSharpAnalyzerTest<ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestLockWithMultipleStatements()
    {
        const string testCode = @"
using System;

public class MyClass
{
    private static int _counter;

    public void MyMethod()
    {
        lock (this)
        {
            _counter = 0; // This should trigger a diagnostic
            Console.WriteLine(""Hello""); // This should not
        }
    }
}
        ";

        var context = new CSharpAnalyzerTest<ReviewLockUsedOnlyForOperationsOnVariablesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ReviewLockUsedOnlyForOperationsOnVariables)
            .WithSpan(10, 15, 10, 19)
            .WithArguments("this");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}