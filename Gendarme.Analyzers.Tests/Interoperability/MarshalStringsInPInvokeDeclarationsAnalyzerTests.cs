using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(MarshalStringsInPInvokeDeclarationsAnalyzer))]
public sealed class MarshalStringsInPInvokeDeclarationsAnalyzerTests
{
    [Fact]
    public async Task TestMissingCharSetAttributeForPInvokeWithStringParameters()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""user32.dll"")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
}
";

        var context = new CSharpAnalyzerTest<MarshalStringsInPInvokeDeclarationsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected1 = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarshalStringsInPInvokeDeclarations)
            .WithSpan(8, 54, 8, 65)
            .WithArguments("text");

        context.ExpectedDiagnostics.Add(expected1);

        var expected2 = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarshalStringsInPInvokeDeclarations)
            .WithSpan(8, 67, 8, 81)
            .WithArguments("caption");

        context.ExpectedDiagnostics.Add(expected2);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestValidPInvokeWithCharSetAttribute()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""user32.dll"", CharSet = CharSet.Ansi)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
}
";

        var context = new CSharpAnalyzerTest<MarshalStringsInPInvokeDeclarationsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics are expected, since CharSet is specified.
        
        await context.RunAsync();
    }

    [Fact]
    public async Task TestPInvokeWithMarshalAsAttribute()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""user32.dll"")]
    public static extern int MessageBox(IntPtr hWnd, [MarshalAs(UnmanagedType.LPStr)] string text, string caption, int options);
}
";

        var context = new CSharpAnalyzerTest<MarshalStringsInPInvokeDeclarationsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarshalStringsInPInvokeDeclarations)
            .WithSpan(8, 100, 8, 114)
            .WithArguments("caption");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}