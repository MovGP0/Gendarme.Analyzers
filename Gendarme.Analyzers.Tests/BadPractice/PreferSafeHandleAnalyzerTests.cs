using Gendarme.Analyzers.BadPractice;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(PreferSafeHandleAnalyzer))]
public sealed class PreferSafeHandleAnalyzerTests
{
    [Fact]
    public async Task TestPreferSafeHandleWarningForIntPtrField()
    {
        const string testCode = @"
using System;
public class MyClass
{
    public IntPtr handle;

    public void UseHandle()
    {
        Console.WriteLine(handle);
    }
}";

        var context = new CSharpAnalyzerTest<PreferSafeHandleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferSafeHandle)
            .WithSpan(6, 14, 6, 20)
            .WithArguments("IntPtr");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestPreferSafeHandleWarningForUIntPtrField()
    {
        const string testCode = @"
using System;
public class MyClass
{
    public UIntPtr uhandle;

    public void UseHandle()
    {
        Console.WriteLine(uhandle);
    }
}";

        var context = new CSharpAnalyzerTest<PreferSafeHandleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.PreferSafeHandle)
            .WithSpan(6, 14, 6, 21)
            .WithArguments("UIntPtr");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNoWarningForSafeHandleField()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public class MySafeHandle : SafeHandle
{
    public MySafeHandle() : base(IntPtr.Zero, true) { }
    public override bool IsInvalid => true;
    protected override bool ReleaseHandle() { return true; }
}

public class MyClass
{
    public MySafeHandle safeHandle;

    public void UseSafeHandle()
    {
        Console.WriteLine(safeHandle);
    }
}";

        var context = new CSharpAnalyzerTest<PreferSafeHandleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        context.ExpectedDiagnostics.Clear(); // No expected diagnostics since there should be no warnings

        await context.RunAsync();
    }
}