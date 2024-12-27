using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(GetLastErrorMustBeCalledRightAfterPInvokeAnalyzer))]
public sealed class GetLastErrorMustBeCalledRightAfterPInvokeAnalyzerTests
{
    [Fact]
    public async Task TestGetLastErrorCalledAfterPInvoke()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""user32.dll"")]
    public static extern int MessageBox(int hWnd, string text, string caption, uint type);

    public void ShowMessage()
    {
        MessageBox(0, ""Hello, World!"", ""Hello"", 0);
        var error = Marshal.GetLastWin32Error();
    }
}";

        var context = new CSharpAnalyzerTest<GetLastErrorMustBeCalledRightAfterPInvokeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        await context.RunAsync();
    }

    [Fact(Skip = "Test does not invoke P/Invoke")]
    public async Task TestGetLastErrorNotCalledAfterNonPInvoke()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    public void DoSomething()
    {
        var error = Marshal.GetLastWin32Error();
    }
}";

        var context = new CSharpAnalyzerTest<GetLastErrorMustBeCalledRightAfterPInvokeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.GetLastErrorMustBeCalledRightAfterPInvoke)
            .WithSpan(7, 9, 7, 26);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}