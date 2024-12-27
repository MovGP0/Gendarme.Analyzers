using Gendarme.Analyzers.Design;

namespace Gendarme.Analyzers.Tests.Design;

[TestOf(typeof(MarkAssemblyWithAssemblyVersionAnalyzer))]
public sealed class MarkAssemblyWithAssemblyVersionAnalyzerTests
{
    [Fact]
    public async Task TestMissingAssemblyVersion()
    {
        const string testCode = @"
using System.Reflection;

public class MyClass { }
";

        var context = new CSharpAnalyzerTest<MarkAssemblyWithAssemblyVersionAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.MarkAssemblyWithAssemblyVersion)
            .WithLocation(1) // adjust location as per your specific case
            .WithArguments("<unnamed>");

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