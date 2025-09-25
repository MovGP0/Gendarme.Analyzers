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
            .WithSpan(6, 18, 6, 22);

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
    public async Task TestMarshalReadInt32WithIntPtr()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass 
{
    public void MyMethod()
    {
        IntPtr ptr = IntPtr.Zero;
        int value = Marshal.ReadInt32(ptr); // This should trigger the diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotAssumeIntPtrSizeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DoNotAssumeIntPtrSize)
            .WithSpan(6, 47, 6, 81);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMarshalReadInt64WithIntPtr()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MyClass 
{
    public void MyMethod()
    {
        IntPtr ptr = IntPtr.Zero;
        long value = Marshal.ReadInt64(ptr); // This should not trigger the diagnostic
    }
}";

        var context = new CSharpAnalyzerTest<DoNotAssumeIntPtrSizeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }
}