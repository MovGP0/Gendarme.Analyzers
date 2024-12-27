using Gendarme.Analyzers.Correctness;

namespace Gendarme.Analyzers.Tests.Correctness;

[TestOf(typeof(DisposableFieldsShouldBeDisposedAnalyzer))]
public sealed class DisposableFieldsShouldBeDisposedAnalyzerTests
{
    [Fact]
    public async Task TestDisposableFieldNotDisposed()
    {
        const string testCode = @"using System;

class MyDisposable : IDisposable
{
    public void Dispose() { }
}

class MyClass
{
    private MyDisposable _myDisposable = new MyDisposable();

    public void Dispose()
    {
        // Missing disposal of _myDisposable
    }
}";

        var context = new CSharpAnalyzerTest<DisposableFieldsShouldBeDisposedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DisposableFieldsShouldBeDisposed)
            .WithSpan(12, 5, 15, 6)
            .WithArguments("_myDisposable");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestDisposableFieldDisposed()
    {
        const string testCode = @"using System;

class MyDisposable : IDisposable
{
    public void Dispose() { }
}

class MyClass
{
    private MyDisposable _myDisposable = new MyDisposable();

    public void Dispose()
    {
        _myDisposable.Dispose();
    }
}";

        var context = new CSharpAnalyzerTest<DisposableFieldsShouldBeDisposedAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics, since the disposable field is disposed correctly.
        await context.RunAsync();
    }
}