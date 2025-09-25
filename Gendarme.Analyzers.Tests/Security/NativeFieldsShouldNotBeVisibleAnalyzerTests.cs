using Gendarme.Analyzers.Security;

namespace Gendarme.Analyzers.Tests.Security;

[TestOf(typeof(NativeFieldsShouldNotBeVisibleAnalyzer))]
public sealed class NativeFieldsShouldNotBeVisibleAnalyzerTests
{
    [Fact]
    public async Task TestPublicNativeField()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass
{
    public IntPtr NativeField; // Should trigger diagnostic
}
";

        var context = new CSharpAnalyzerTest<NativeFieldsShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.NativeFieldsShouldNotBeVisible)
            .WithSpan(7, 19, 7, 30) // Span for the NativeField identifier (exclusive end)
            .WithArguments("NativeField", "nint");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPublicSafeHandleField()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass
{
    public SafeHandle HandleField; // Should trigger diagnostic
}
";

        var context = new CSharpAnalyzerTest<NativeFieldsShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.NativeFieldsShouldNotBeVisible)
            .WithSpan(7, 23, 7, 34) // Span for the HandleField identifier (exclusive end)
            .WithArguments("HandleField", "System.Runtime.InteropServices.SafeHandle");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonPublicFieldDoesNotTrigger()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass
{
    private IntPtr NativeField; // Should NOT trigger diagnostic
}
";

        var context = new CSharpAnalyzerTest<NativeFieldsShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics for this test
        await context.RunAsync();
    }
}