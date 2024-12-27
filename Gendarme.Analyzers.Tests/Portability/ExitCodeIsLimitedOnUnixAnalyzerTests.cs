using Gendarme.Analyzers.Portability;

namespace Gendarme.Analyzers.Tests.Portability;

[TestOf(typeof(ExitCodeIsLimitedOnUnixAnalyzer))]
public sealed class ExitCodeIsLimitedOnUnixAnalyzerTests
{
    [Fact]
    public async Task TestReturnStatement_ExitCodeOutOfRange()
    {
        const string testCode = @"
        public class Program
        {
            public static int Main()
            {
                return 300; // Out of range
            }
        }";

        var context = new CSharpAnalyzerTest<ExitCodeIsLimitedOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ExitCodeIsLimitedOnUnix)
            .WithSpan(6, 17, 6, 28);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAssignment_ExitCodeOutOfRange()
    {
        const string testCode = @"
using System;

public class Program
{
    public static void Main()
    {
        Environment.ExitCode = 256; // Out of range
    }
}";

        var context = new CSharpAnalyzerTest<ExitCodeIsLimitedOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ExitCodeIsLimitedOnUnix)
            .WithSpan(8, 9, 8, 35);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvocation_ExitMethodOutOfRange()
    {
        const string testCode = @"
using System;

public class Program
{
    public static void Main()
    {
        Environment.Exit(300); // Out of range
    }
}";

        var context = new CSharpAnalyzerTest<ExitCodeIsLimitedOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.ExitCodeIsLimitedOnUnix)
            .WithSpan(8, 9, 8, 30);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestReturnStatement_ValidExitCode()
    {
        const string testCode = @"
        public class Program
        {
            public static int Main()
            {
                return 0; // Valid exit code
            }
        }";

        var context = new CSharpAnalyzerTest<ExitCodeIsLimitedOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAssignment_ValidExitCode()
    {
        const string testCode = @"
using System;

public class Program
{
    public static void Main()
    {
        Environment.ExitCode = 255; // Valid exit code
    }
}";

        var context = new CSharpAnalyzerTest<ExitCodeIsLimitedOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInvocation_ValidExitCode()
    {
        const string testCode = @"
using System;

public class Program
{
    public static void Main()
    {
        Environment.Exit(1); // Valid exit code
    }
}";

        var context = new CSharpAnalyzerTest<ExitCodeIsLimitedOnUnixAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}