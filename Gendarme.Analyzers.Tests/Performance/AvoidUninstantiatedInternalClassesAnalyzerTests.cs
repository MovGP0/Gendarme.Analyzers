using Gendarme.Analyzers.Performance;

namespace Gendarme.Analyzers.Tests.Performance;

[TestOf(typeof(AvoidUninstantiatedInternalClassesAnalyzer))]
public sealed class AvoidUninstantiatedInternalClassesAnalyzerTests
{
    [Fact]
    public async Task TestUninstantiatedInternalClass()
    {
        const string testCode = @"
namespace TestNamespace
{
    internal class UninstantiatedClass { }
}
";

        var context = new CSharpAnalyzerTest<AvoidUninstantiatedInternalClassesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        var expected = new DiagnosticResult(DiagnosticId.AvoidUninstantiatedInternalClasses, DiagnosticSeverity.Info)
            .WithSpan(4, 20, 4, 39)
            .WithArguments("UninstantiatedClass");

        context.ExpectedDiagnostics.Add(expected);

        await context.RunAsync();
    }

    [Fact]
    public async Task TestInstantiatedInternalClass()
    {
        const string testCode = @"
namespace TestNamespace
{
    internal class InstantiatedClass
    {
        public InstantiatedClass() { }
    }

    public class Test
    {
        public void Method()
        {
            var instance = new InstantiatedClass();
        }
    }
}
";

        var context = new CSharpAnalyzerTest<AvoidUninstantiatedInternalClassesAnalyzer, DefaultVerifier>
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            TestCode = testCode
        };

        // No diagnostics expected because the internal class is instantiated
        await context.RunAsync();
    }
}