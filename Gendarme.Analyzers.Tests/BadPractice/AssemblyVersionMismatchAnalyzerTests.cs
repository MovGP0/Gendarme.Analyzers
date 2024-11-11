using Gendarme.Analyzers.BadPractice;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Gendarme.Analyzers.Tests.BadPractice;

[TestOf(typeof(AssemblyVersionMismatchAnalyzer))]
public sealed class AssemblyVersionMismatchAnalyzerTests
{
    [Fact]
    public async Task TestVersionMismatch()
    {
        const string testCode = """
using System.Reflection;
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0.0")]

public class MyClass { }
""";

        var context = new CSharpAnalyzerTest<AssemblyVersionMismatchAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode 
        };

        var expected = DiagnosticResult
            .CompilerWarning(DiagnosticId.AssemblyVersionMismatch)
            .WithSpan(6, 14, 6, 21)
            .WithArguments("1.0.0.0");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }
}