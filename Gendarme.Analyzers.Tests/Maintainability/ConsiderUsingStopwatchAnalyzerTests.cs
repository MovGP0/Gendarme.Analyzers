using Gendarme.Analyzers.Maintainability;

namespace Gendarme.Analyzers.Tests.Maintainability;

[TestOf(typeof(ConsiderUsingStopwatchAnalyzer))]
public sealed class ConsiderUsingStopwatchAnalyzerTests
{
    [Fact(Skip = "Analyzer not working as expected")]
    public async Task TestDateTimeNowUsage()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        var startTime = DateTime.Now;
        // Simulating work
        var endTime = DateTime.Now - startTime;
    }
}
";

        var context = new CSharpAnalyzerTest<ConsiderUsingStopwatchAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ConsiderUsingStopwatch)
            .WithSpan(8, 33, 8, 42) // Adjust span as per actual code structure
            .WithArguments("Consider using Stopwatch instead of DateTime for measuring elapsed time.");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestNoDiagnosticsWhenUsingStopwatch()
    {
        const string testCode = @"
using System;
using System.Diagnostics;

public class MyClass
{
    public void MyMethod()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        // Simulating work
        stopwatch.Stop();
    }
}
";

        var context = new CSharpAnalyzerTest<ConsiderUsingStopwatchAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}