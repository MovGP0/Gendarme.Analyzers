using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(DoNotAssumeIntPtrSizeAnalyzer))]
public sealed class DoNotAssumeIntPtrSizeAnalyzerTests
{
    [Fact]
    public async Task TestCastToInt32FromIntPtr()
    {
        const string testCode = @"
using System;

public class MyClass 
{
    public void MyMethod()
    {
        IntPtr ptr = IntPtr.Zero;
        int value = (int)ptr; // This should trigger the diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotAssumeIntPtrSizeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotAssumeIntPtrSize)
            .WithSpan(9, 21, 9, 29); // (int)ptr

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCastToInt64FromIntPtr()
    {
        const string testCode = @"
using System;

public class MyClass 
{
    public void MyMethod()
    {
        IntPtr ptr = IntPtr.Zero;
        long value = (long)ptr; // This should not trigger the diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotAssumeIntPtrSizeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMarshalReadInt32CastToIntPtr()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass 
{
    public void MyMethod()
    {
        IntPtr ptr = IntPtr.Zero;
        IntPtr value = (IntPtr)Marshal.ReadInt32(ptr); // Should trigger
    }
}";

        var context = new CSharpAnalyzerTest<DoNotAssumeIntPtrSizeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotAssumeIntPtrSize)
            .WithSpan(10, 32, 10, 54); // Marshal.ReadInt32(ptr)

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMarshalReadInt64CastToIntPtr()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass 
{
    public void MyMethod()
    {
        IntPtr ptr = IntPtr.Zero;
        IntPtr value = (IntPtr)Marshal.ReadInt64(ptr); // Should trigger
    }
}";

        var context = new CSharpAnalyzerTest<DoNotAssumeIntPtrSizeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotAssumeIntPtrSize)
            .WithSpan(10, 32, 10, 54); // Marshal.ReadInt64(ptr)

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}