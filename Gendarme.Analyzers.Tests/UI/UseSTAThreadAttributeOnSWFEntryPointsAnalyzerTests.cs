using Gendarme.Analyzers.UI;

namespace Gendarme.Analyzers.Tests.UI;

[TestOf(typeof(UseStaThreadAttributeOnSwfEntryPointsAnalyzer))]
public sealed class UseStaThreadAttributeOnSwfEntryPointsAnalyzerTests
{
    [Fact]
    public async Task TestMissingSTAThreadAttribute()
    {
        const string testCode = @"
using System;
using System.Windows.Forms;

public class Program
{
    // Missing STAThread attribute
    public static void Main()
    {
        Application.Run(new Form());
    }
}
";

        var context = new CSharpAnalyzerTest<UseStaThreadAttributeOnSwfEntryPointsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
            TestCode = testCode,
            TestState = { OutputKind = OutputKind.ConsoleApplication }
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseStaThreadAttributeOnSwfEntryPoints)
            .WithSpan(8, 24, 8, 28);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMTAThreadAttribute()
    {
        const string testCode = @"
using System;
using System.Windows.Forms;

public class Program
{
    [MTAThread] // Using MTAThread instead of STAThread
    public static void Main()
    {
        Application.Run(new Form());
    }
}
";

        var context = new CSharpAnalyzerTest<UseStaThreadAttributeOnSwfEntryPointsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
            TestCode = testCode,
            TestState = { OutputKind = OutputKind.ConsoleApplication }
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.UseStaThreadAttributeOnSwfEntryPoints)
            .WithSpan(8, 24, 8, 28);

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestSTAThreadAttribute()
    {
        const string testCode = @"
using System;
using System.Windows.Forms;

public class Program
{
    [STAThread] // Correct usage of STAThread attribute
    public static void Main()
    {
        Application.Run(new Form());
    }
}
";

        var context = new CSharpAnalyzerTest<UseStaThreadAttributeOnSwfEntryPointsAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80Windows,
            TestCode = testCode,
            TestState = { OutputKind = OutputKind.ConsoleApplication }
        };

        // No expected diagnostics as the usage is correct
        await context.RunAsync();
    }
}