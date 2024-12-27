using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(MarkAssemblyWithComVisibleAnalyzer))]
public sealed class MarkAssemblyWithComVisibleAnalyzerTests
{
    [Fact]
    public async Task TestMissingComVisibleAttribute()
    {
        const string testCode = @"
[assembly: System.Reflection.AssemblyTitle(""TestAssembly"")]

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MarkAssemblyWithComVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarkAssemblyWithComVisible)
            .WithSpan(3, 14, 3, 31)
            .WithArguments("TestAssembly");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestComVisibleAttributePresent()
    {
        const string testCode = @"
using System.Runtime.InteropServices;

[assembly: ComVisible(true)]
[assembly: System.Reflection.AssemblyTitle(""TestAssembly"")]

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MarkAssemblyWithComVisibleAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected, so nothing is added to ExpectedDiagnostics

        await context.RunAsync();
    }
}