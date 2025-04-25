using Gendarme.Analyzers.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(MarkAssemblyWithAssemblyVersionAnalyzer))]
public sealed class MarkAssemblyWithAssemblyVersionAnalyzerTests
{
    [Fact]
    public async Task TestMissingAssemblyVersion()
    {
        const string testCode = @"
// No assembly version attribute at all
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MarkAssemblyWithAssemblyVersionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode,
            TestState = { OutputKind = OutputKind.DynamicallyLinkedLibrary }  // Ensure it's treated as a library
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarkAssemblyWithAssemblyVersion)
            .WithArguments("TestProject");  // Default project name in test context

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestWithAssemblyVersion()
    {
        const string testCode = @"
using System.Reflection;

[assembly: AssemblyVersion(""1.0.0.0"")]
public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MarkAssemblyWithAssemblyVersionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No expected diagnostics since the assembly version is present
        await context.RunAsync();
    }
}