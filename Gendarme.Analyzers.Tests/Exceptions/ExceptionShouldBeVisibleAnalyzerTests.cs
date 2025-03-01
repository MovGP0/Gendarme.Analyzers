using Gendarme.Analyzers.Exceptions;

namespace Gendarme.Analyzers.Tests.Exceptions;

[TestOf(typeof(ExceptionShouldBeVisibleAnalyzer))]
public sealed class ExceptionShouldBeVisibleAnalyzerTests
{
    [Fact]
    public async Task TestPublicExceptionIsIgnored()
    {
        const string testCode = @"
using System;

public class MyPublicException : Exception { }
";

        var context = new CSharpAnalyzerTest<ExceptionShouldBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonPublicExceptionRaisesWarning()
    {
        const string testCode = @"
using System;

internal class MyNonPublicException : Exception { }
";

        var context = new CSharpAnalyzerTest<ExceptionShouldBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ExceptionShouldBeVisible)
            .WithSpan(4, 16, 4, 36)
            .WithArguments("MyNonPublicException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonPublicInheritedExceptionRaisesWarning()
    {
        const string testCode = @"
using System;

internal class MyApplicationException : ApplicationException { }
";

        var context = new CSharpAnalyzerTest<ExceptionShouldBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ExceptionShouldBeVisible)
            .WithSpan(4, 16, 4, 38)
            .WithArguments("MyApplicationException");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicInheritedExceptionIsIgnored()
    {
        const string testCode = @"
using System;

public class MySystemException : SystemException { }
";

        var context = new CSharpAnalyzerTest<ExceptionShouldBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}