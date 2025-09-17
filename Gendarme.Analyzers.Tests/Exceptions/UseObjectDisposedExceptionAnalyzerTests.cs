using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(UseObjectDisposedExceptionAnalyzer))]
public sealed class UseObjectDisposedExceptionAnalyzerTests
{
    [Fact]
    public async Task TestMissingDisposedCheckInMethod()
    {
        const string testCode = @"
using System;

public class MyClass : IDisposable
{
    private bool disposed = false;

    public void SomeMethod()
    {
        // Missing disposed check
    }

    public void Dispose()
    {
        disposed = true;
    }
}
";

        var context = new CSharpAnalyzerTest<UseObjectDisposedExceptionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult.CompilerWarning(DiagnosticId.UseObjectDisposedException)
            .WithSpan(8, 17, 8, 27) // Start at SomeMethod() line
            .WithArguments("SomeMethod");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCorrectDisposedCheckInMethod()
    {
        const string testCode = @"
using System;

public class MyClass : IDisposable
{
    private bool disposed = false;

    public void SomeMethod()
    {
        if (disposed) throw new ObjectDisposedException(nameof(MyClass));
        // Do something
    }

    public void Dispose()
    {
        disposed = true;
    }
}
";

        var context = new CSharpAnalyzerTest<UseObjectDisposedExceptionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}