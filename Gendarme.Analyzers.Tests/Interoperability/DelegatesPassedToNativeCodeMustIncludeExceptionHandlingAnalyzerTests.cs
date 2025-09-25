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
        ExternalMethod(Callback);
    }

    public void Callback()
    {
        Console.WriteLine(""Hello"");
    }
}";

        var context = new CSharpAnalyzerTest<DelegatesPassedToNativeCodeMustIncludeExceptionHandlingAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.DelegatesPassedToNativeCodeMustIncludeExceptionHandling)
            .WithSpan(17, 17, 17, 25)
            .WithArguments("Callback");

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
        ExternalMethod(Callback);
    }

    public void Callback()
    {
        try
        {
            Console.WriteLine(""Hello"");
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

        await context.RunAsync();
    }
}