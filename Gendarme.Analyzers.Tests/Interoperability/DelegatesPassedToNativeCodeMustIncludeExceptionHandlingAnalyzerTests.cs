using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzer))]
public sealed class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzerTests
{
    [Fact]
    public async Task TestMethodWithoutCatchAll()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public delegate void MyDelegate();

public class MyClass
{
    [DllImport(""SomeLibrary.dll"")]
    public static extern void ExternalMethod(MyDelegate del);

    public void MethodWithoutCatchAll()
    {
        ExternalMethod(() => { throw new Exception(); });
    }
}";

        var context = new CSharpAnalyzerTest<DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DelegatesPassedToNativeCodeMustIncludeExceptionHandling)
            .WithSpan(10, 9, 10, 31)
            .WithArguments("MethodWithoutCatchAll");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodWithCatchAll()
    {
        const string testCode = @"
using System;
using System.Runtime.InteropServices;

public delegate void MyDelegate();

public class MyClass
{
    [DllImport(""SomeLibrary.dll"")]
    public static extern void ExternalMethod(MyDelegate del);

    public void MethodWithCatchAll()
    {
        try
        {
            ExternalMethod(() => { throw new Exception(); });
        }
        catch
        {
            // Catch all exceptions
        }
    }
}";

        var context = new CSharpAnalyzerTest<DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics are expected since the method has catch-all exception handling.
        
        await context.RunAsync();
    }
}