using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(MarkAssemblyWithClsCompliantAnalyzer))]
public sealed class MarkAssemblyWithClsCompliantAnalyzerTests
{
    [Fact]
    public async Task TestAssemblyWithoutClsCompliance()
    {
        const string testCode = @"
[assembly: System.CLSCompliant(false)]

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MarkAssemblyWithClsCompliantAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarkAssemblyWithClsCompliant)
            .WithLocation(1) // adjust location based on the actual line
            .WithArguments("<unnamed>");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestAssemblyWithClsCompliance()
    {
        const string testCode = @"
[assembly: System.CLSCompliant(true)]

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MarkAssemblyWithClsCompliantAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected for compliant assembly
        await context.RunAsync();
    }
}