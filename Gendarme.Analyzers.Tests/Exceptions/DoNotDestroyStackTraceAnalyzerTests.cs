using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(DoNotDestroyStackTraceAnalyzer))]
public sealed class DoNotDestroyStackTraceAnalyzerTests
{
    [Fact]
    public async Task TestDoNotDestroyStackTrace()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        try
        {
            throw new InvalidOperationException();
        }
        catch (InvalidOperationException ex)
        {
            // Rethrow the same exception
            throw ex;
        }
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotDestroyStackTraceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotDestroyStackTrace)
            .WithSpan(15, 13, 15, 22); // Adjusted to match the line number of "throw ex;" statement in the test code

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidThrowStatement()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        throw new InvalidOperationException();
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotDestroyStackTraceAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}