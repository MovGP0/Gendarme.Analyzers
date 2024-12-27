using Gendarme.Analyzers.UI;

namespace Gendarme.Analyzers.Tests.UI;

[TestOf(typeof(SystemWindowsFormsExecutableTargetAnalyzer))]
public sealed class SystemWindowsFormsExecutableTargetAnalyzerTests
{
    [Fact(Skip = "Was unable to compile as .exe")]
    public async Task TestConsoleApplicationWithWinFormsReference()
    {
        const string testCode = @"
using System;
using System.Windows.Forms;

class Program
{
    static void Main()
    {
        Application.Run(new Form());
    }
}";

        var context = new CSharpAnalyzerTest<SystemWindowsFormsExecutableTargetAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.SystemWindowsFormsExecutableTarget)
            .WithSpan(1, 1, 6, 1);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestNonConsoleApplicationWithWinFormsReference()
    {
        const string testCode = @"
using System.Windows.Forms;

class MyForm : Form
{
}";

        var context = new CSharpAnalyzerTest<SystemWindowsFormsExecutableTargetAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
            TestCode = testCode
        };

        // No diagnostics expected for non-console applications
        await context.RunAsync();
    }

    [Fact]
    public async Task TestConsoleApplicationWithoutWinFormsReference()
    {
        const string testCode = @"
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(""Hello, World!"");
    }
}";

        var context = new CSharpAnalyzerTest<SystemWindowsFormsExecutableTargetAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected since there is no WinForms reference
        await context.RunAsync();
    }
}