using Gendarme.Analyzers.Interoperability;

namespace Gendarme.Analyzers.Tests.Interoperability;

[TestOf(typeof(UseManagedAlternativesToPInvokeAnalyzer))]
public sealed class UseManagedAlternativesToPInvokeAnalyzerTests
{
    [Fact]
    public async Task TestPInvokeUsage()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

public class MyClass
{
    [DllImport(""kernel32.dll"")]
    public static extern void Sleep(uint milliseconds);

    public void Test()
    {
        Sleep(1000);
    }
}";

        var context = new CSharpAnalyzerTest<UseManagedAlternativesToPInvokeAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseManagedAlternativesToPInvoke)
            .WithSpan(11, 9, 11, 20)
            .WithArguments("Sleep", "System.Threading.Thread.Sleep");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}