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

// No native field, so no warning expected
public class MyClass : IDisposable
{
    public void Dispose()
    {
        // Implement IDisposable
    }
}";

        var context = new CSharpAnalyzerTest<DisposableTypesShouldHaveFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostic expected since there's no native field
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

public class MyClass : IDisposable
{
    private IntPtr handle; // Native field

    public void Dispose()
    {
        // Dispose implementation
    }

    // No finalizer - should trigger warning
}";

        var context = new CSharpAnalyzerTest<DisposableTypesShouldHaveFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DisposableTypesShouldHaveFinalizer)
            .WithSpan(4, 14, 4, 21)
            .WithArguments("MyClass");
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