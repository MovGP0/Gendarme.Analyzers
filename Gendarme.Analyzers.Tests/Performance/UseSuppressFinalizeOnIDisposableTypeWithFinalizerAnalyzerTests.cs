using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer))]
public sealed class UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzerTests
{
    [Fact]
    public async Task TestIDisposableWithFinalizerWithoutSuppressFinalize()
    {
        const string testCode = @"
public class MyClass : System.IDisposable
{
    ~MyClass()
    {
    }

    public void Dispose()
    {
        // Missing SuppressFinalize call
    }
}";

        var context = new CSharpAnalyzerTest<UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseSuppressFinalizeOnIDisposableTypeWithFinalizer)
            .WithSpan(5, 14, 5, 22)
            .WithArguments("MyClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestIDisposableWithFinalizerWithSuppressFinalize()
    {
        const string testCode = @"
public class MyClass : System.IDisposable
{
    ~MyClass()
    {
    }

    public void Dispose()
    {
        System.GC.SuppressFinalize(this);
    }
}";

        var context = new CSharpAnalyzerTest<UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonIDisposableClassWithFinalizer()
    {
        const string testCode = @"
public class MyClass
{
    ~MyClass()
    {
    }
}";

        var context = new CSharpAnalyzerTest<UseSuppressFinalizeOnIDisposableTypeWithFinalizerAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected
        await context.RunAsync();
    }
}