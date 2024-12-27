using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(EnsureLocalDisposalAnalyzer))]
public sealed class EnsureLocalDisposalAnalyzerTests
{
    [Fact]
    public async Task TestLocalDisposalWarning()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        var disposable1 = new DisposableResource();
        var disposable2 = new DisposableResource();
        // Missing disposal calls, should trigger a warning
    }
}

public class DisposableResource : IDisposable
{
    public void Dispose() { }
}
";

        var context = new CSharpAnalyzerTest<EnsureLocalDisposalAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.EnsureLocalDisposal)
            .WithSpan(8, 13, 8, 51)
            .WithArguments("disposable1");

        context.ExpectedDiagnostics.Add(expected1);

        var expected2 = DiagnosticResult
            .CompilerWarning(DiagnosticId.EnsureLocalDisposal)
            .WithSpan(9, 13, 9, 51)
            .WithArguments("disposable2");

        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestLocalDisposalWithDisposeCall()
    {
        const string testCode = @"
using System;

public class MyClass
{
    public void MyMethod()
    {
        var disposable = new DisposableResource();
        disposable.Dispose(); // Proper disposal, should not trigger a warning
    }
}

public class DisposableResource : IDisposable
{
    public void Dispose() { }
}
";

        var context = new CSharpAnalyzerTest<EnsureLocalDisposalAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since disposal is handled
        await context.RunAsync();
    }
}