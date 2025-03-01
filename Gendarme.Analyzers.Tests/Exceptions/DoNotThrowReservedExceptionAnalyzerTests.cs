using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(DoNotThrowReservedExceptionAnalyzer))]
public sealed class DoNotThrowReservedExceptionAnalyzerTests
{
    [Fact]
    public async Task TestThrowReservedException()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        throw new NullReferenceException();
    }
}
";

        var context = new CSharpAnalyzerTest<DoNotThrowReservedExceptionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotThrowReservedException)
            .WithSpan(8, 15, 8, 43)
            .WithArguments("NullReferenceException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestThrowNonReservedException()
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

        var context = new CSharpAnalyzerTest<DoNotThrowReservedExceptionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}