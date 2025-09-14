using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(MarkAssemblyWithClsCompliantAnalyzer))]
public sealed class MarkAssemblyWithClsCompliantAnalyzerTests
{
    [Fact]
    public async Task TestAssemblyWithoutClsCompliance()
    {
        const string testCode = @"
// No CLSCompliant attribute at all
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MarkAssemblyWithClsCompliantAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode,
            TestState = { OutputKind = OutputKind.DynamicallyLinkedLibrary }  // Ensure it's treated as a library
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarkAssemblyWithClsCompliant)
            .WithArguments("TestProject");  // Default project name in test context

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