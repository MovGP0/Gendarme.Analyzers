using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(MarshalBooleansInPInvokeDeclarationsAnalyzer))]
public sealed class MarshalBooleansInPInvokeDeclarationsAnalyzerTests
{
    [Fact]
    public async Task TestBooleanParameterWithoutMarshalAs()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""SomeDll.dll"")]
    public static extern void SomeMethod(bool myBool);
}
";

        var context = new CSharpAnalyzerTest<MarshalBooleansInPInvokeDeclarationsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarshalBooleansInPInvokeDeclarations)
            .WithSpan(7, 42, 7, 53)
            .WithArguments("myBool");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestBooleanReturnValueWithoutMarshalAs()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""SomeDll.dll"")]
    public static extern bool SomeMethod();
}
";

        var context = new CSharpAnalyzerTest<MarshalBooleansInPInvokeDeclarationsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarshalBooleansInPInvokeDeclarations)
            .WithSpan(6, 5, 7, 44)
            .WithArguments("return value");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestBooleanParameterWithMarshalAs()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""SomeDll.dll"")]
    public static extern void SomeMethod([MarshalAs(UnmanagedType.I1)] bool myBool);
}
";

        var context = new CSharpAnalyzerTest<MarshalBooleansInPInvokeDeclarationsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since MarshalAs is present
        await context.RunAsync();
    }

    [Fact]
    public async Task TestBooleanReturnValueWithMarshalAs()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""SomeDll.dll"")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SomeMethod();
}
";

        var context = new CSharpAnalyzerTest<MarshalBooleansInPInvokeDeclarationsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since MarshalAs is present
        await context.RunAsync();
    }
}