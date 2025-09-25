using Gendarme.Analyzers.Concurrency;

namespace Gendarme.Analyzers.Tests.Concurrency;

[TestOf(typeof(DoNotLockOnWeakIdentityObjectsAnalyzer))]
public sealed class DoNotLockOnWeakIdentityObjectsAnalyzerTests
{
    [Fact]
    public async Task DetectsLockOnStringLiteral()
    {
        const string testCode = @"using System;

public class MyClass
{
    public void MyMethod()
    {
        lock (string.Empty)
        {
            Console.WriteLine();
        }
    }
}";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotLockOnWeakIdentityObjects)
            .WithSpan(7, 15, 7, 27);

        await VerifyAsync(testCode, expected);
    }

    [Fact]
    public async Task DetectsLockOnTypeInstance()
    {
        const string testCode = @"using System;

public class MyClass
{
    public void MyMethod()
    {
        lock (typeof(string))
        {
            Console.WriteLine();
        }
    }
}";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotLockOnWeakIdentityObjects)
            .WithSpan(7, 15, 7, 29);

        await VerifyAsync(testCode, expected);
    }

    [Fact]
    public async Task DetectsLockOnThreadInstance()
    {
        const string testCode = @"using System.Threading;

public class MyClass
{
    private readonly Thread _thread = Thread.CurrentThread;

    public void MyMethod()
    {
        lock (_thread)
        {
            _ = 0;
        }
    }
}";

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotLockOnWeakIdentityObjects)
            .WithSpan(9, 15, 9, 22);

        await VerifyAsync(testCode, expected);
    }

    [Fact]
    public async Task SkipsLockOnDedicatedObject()
    {
        const string testCode = @"using System;

public class MyClass
{
    private readonly object _gate = new object();

    public void MyMethod()
    {
        lock (_gate)
        {
            Console.WriteLine();
        }
    }
}";

        await VerifyAsync(testCode);
    }

    private static Task VerifyAsync(string source, params DiagnosticResult[] expectedDiagnostics)
    {
        var test = new CSharpAnalyzerTest<DoNotLockOnWeakIdentityObjectsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = source
        };

        test.ExpectedDiagnostics.AddRange(expectedDiagnostics);
        return test.RunAsync();
    }
}

