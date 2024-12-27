using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(DisposableTypesShouldHaveFinalizerAnalyzer))]
public sealed class DisposableTypesShouldHaveFinalizerAnalyzerTests
{
    [Fact]
    public async Task TestDisposableWithoutFinalizerReportsWarning()
    {
        const string testCode = @"
using System;

public class MyDisposableClass : IDisposable
{
    public void Dispose() { }
}
";

        var context = new CSharpAnalyzerTest<DisposableTypesShouldHaveFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DisposableTypesShouldHaveFinalizer)
            .WithSpan(5, 14, 5, 36)
            .WithArguments("MyDisposableClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestDisposableWithFinalizerDoesNotReportWarning()
    {
        const string testCode = @"
using System;

public class MyDisposableClass : IDisposable
{
    public void Dispose() { }

    ~MyDisposableClass() { }
}
";

        var context = new CSharpAnalyzerTest<DisposableTypesShouldHaveFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestDisposableWithNativeFieldReportsWarning()
    {
        const string testCode = @"
using System;

public class MyNativeDisposableClass : IDisposable
{
    private IntPtr _nativeResource;

    public void Dispose() { }
}
";

        var context = new CSharpAnalyzerTest<DisposableTypesShouldHaveFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DisposableTypesShouldHaveFinalizer)
            .WithSpan(5, 14, 5, 40)
            .WithArguments("MyNativeDisposableClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestDisposableWithNativeFieldAndFinalizerDoesNotReportWarning()
    {
        const string testCode = @"
using System;

public class MyNativeDisposableClass : IDisposable
{
    private IntPtr _nativeResource;

    public void Dispose() { }

    ~MyNativeDisposableClass() { }
}
";

        var context = new CSharpAnalyzerTest<DisposableTypesShouldHaveFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}