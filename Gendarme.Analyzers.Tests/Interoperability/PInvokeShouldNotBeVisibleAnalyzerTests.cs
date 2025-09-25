using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(PInvokeShouldNotBeVisibleAnalyzer))]
public sealed class PInvokeShouldNotBeVisibleAnalyzerTests
{
    [Fact]
    public async Task TestPublicPInvokeMethodShouldBeReported()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""user32.dll"")]
    public static extern int MessageBox(int hWnd, string text, string caption, int type);
}
";

        var context = new CSharpAnalyzerTest<PInvokeShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PInvokeShouldNotBeVisible)
            .WithSpan(7, 30, 7, 40)
            .WithArguments("MessageBox");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestProtectedPInvokeMethodShouldBeReported()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""user32.dll"")]
    protected static extern int MessageBox(int hWnd, string text, string caption, int type);
}
";

        var context = new CSharpAnalyzerTest<PInvokeShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PInvokeShouldNotBeVisible)
            .WithSpan(7, 33, 7, 43)
            .WithArguments("MessageBox");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestProtectedInternalPInvokeMethodShouldBeReported()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""user32.dll"")]
    protected internal static extern int MessageBox(int hWnd, string text, string caption, int type);
}
";

        var context = new CSharpAnalyzerTest<PInvokeShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PInvokeShouldNotBeVisible)
            .WithSpan(7, 42, 7, 52)
            .WithArguments("MessageBox");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPrivatePInvokeMethodShouldNotBeReported()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""user32.dll"")]
    private static extern int MessageBox(int hWnd, string text, string caption, int type);
}
";

        var context = new CSharpAnalyzerTest<PInvokeShouldNotBeVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics for private methods
        await context.RunAsync();
    }
}