namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(CheckNewThreadWithoutStartAnalyzer))]
public sealed class CheckNewThreadWithoutStartAnalyzerTests
{
    [Fact]
    public async Task TestNewThreadWithoutStart()
    {
        const string testCode = @"
        using System.Threading;

        public class MyClass
        {
            public void MyMethod()
            {
                Thread thread = new Thread(() => { /* do work */ });
                // No call to Start()
            }
        }";

        var context = new CSharpAnalyzerTest<CheckNewThreadWithoutStartAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CheckNewThreadWithoutStart)
            .WithSpan(8, 17, 8, 23)
            .WithArguments("thread");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNewThreadWithStart()
    {
        const string testCode = @"
        using System.Threading;

        public class MyClass
        {
            public void MyMethod()
            {
                Thread thread = new Thread(() => { /* do work */ });
                thread.Start(); // Proper usage
            }
        }";

        var context = new CSharpAnalyzerTest<CheckNewThreadWithoutStartAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync(); // No diagnostics expected
    }

    [Fact]
    public async Task TestThreadReturned()
    {
        const string testCode = @"
        using System.Threading;

        public class MyClass
        {
            public Thread MyMethod()
            {
                Thread thread = new Thread(() => { /* do work */ });
                return thread; // Returned without starting
            }
        }";

        var context = new CSharpAnalyzerTest<CheckNewThreadWithoutStartAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CheckNewThreadWithoutStart)
            .WithSpan(8, 17, 8, 23)
            .WithArguments("thread");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestThreadPassedAsArgument()
    {
        const string testCode = @"
        using System.Threading;

        public class MyClass
        {
            public void MyMethod(Thread thread)
            {
                // Thread is passed as argument without starting
            }

            public void CallMethod()
            {
                Thread thread = new Thread(() => { /* do work */ });
                MyMethod(thread);
            }
        }";

        var context = new CSharpAnalyzerTest<CheckNewThreadWithoutStartAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.CheckNewThreadWithoutStart)
            .WithSpan(12, 17, 12, 23)
            .WithArguments("thread");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}